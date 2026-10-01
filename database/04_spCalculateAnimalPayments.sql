USE dbRecruta;
GO

CREATE OR ALTER PROCEDURE dbo.spCalculateAnimalPayments
     @MaleArrobaPrice     NUMERIC(18,2) = 100.00
    ,@FemaleArrobaPrice   NUMERIC(18,2) = 50.00
    ,@PremiumPercentage   NUMERIC(5,2)  = 10.00
    ,@DiscountPercentage  NUMERIC(5,2)  = 10.00
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    DECLARE @KgPerArroba    NUMERIC(5,2) = 15.00;
    DECLARE @CalculatedAt   DATETIME2(0) = SYSDATETIME();
    DECLARE @PremiumReason  VARCHAR(100) = CONCAT('Ágio de ', FORMAT(@PremiumPercentage, '0.##', 'pt-BR'), '%: animal com 0 dentes');
    DECLARE @DiscountReason VARCHAR(100) = CONCAT('Deságio de ', FORMAT(@DiscountPercentage, '0.##', 'pt-BR'), '%: animal com 6 ou mais dentes');
    DECLARE @ProcessedCount INT;
    DECLARE @SkippedCount   INT;

    BEGIN TRY
        BEGIN TRANSACTION;

        DELETE FROM dbo.AnimalPayments;

        INSERT INTO dbo.AnimalPayments
            (AnimalId, OrderId, Sex, TeethCount, Weight, Arrobas, BaseArrobaPrice,
             BaseAmount, PremiumAmount, DiscountAmount, AmountToPay, AdjustmentType, AdjustmentReason, CalculatedAt)
        SELECT
             a.AnimalId
            ,a.OrderId
            ,a.Sex
            ,a.TeethCount
            ,a.Weight
            ,CAST(a.Weight / @KgPerArroba AS NUMERIC(18,4))
            ,price.BaseArrobaPrice
            ,base.BaseAmount
            ,adj.PremiumAmount
            ,adj.DiscountAmount
            ,base.BaseAmount + adj.PremiumAmount - adj.DiscountAmount
            ,adjType.AdjustmentType
            ,CASE adjType.AdjustmentType
                 WHEN 'A' THEN @PremiumReason
                 WHEN 'D' THEN @DiscountReason
                 ELSE CONCAT('Sem ágio ou deságio: animal com ', a.TeethCount, ' dentes')
             END
            ,@CalculatedAt
        FROM dbo.Animals AS a
        CROSS APPLY (SELECT CASE a.Sex WHEN 'M' THEN @MaleArrobaPrice
                                       ELSE @FemaleArrobaPrice END AS BaseArrobaPrice) AS price
        CROSS APPLY (SELECT CAST(a.Weight * price.BaseArrobaPrice / @KgPerArroba AS NUMERIC(18,2)) AS BaseAmount) AS base
        CROSS APPLY (SELECT CASE WHEN a.TeethCount = 0  THEN 'A'
                                 WHEN a.TeethCount >= 6 THEN 'D'
                                 ELSE 'N' END AS AdjustmentType) AS adjType
        CROSS APPLY (SELECT CAST(CASE WHEN adjType.AdjustmentType = 'A'
                                      THEN base.BaseAmount * @PremiumPercentage / 100
                                      ELSE 0 END AS NUMERIC(18,2)) AS PremiumAmount
                           ,CAST(CASE WHEN adjType.AdjustmentType = 'D'
                                      THEN base.BaseAmount * @DiscountPercentage / 100
                                      ELSE 0 END AS NUMERIC(18,2)) AS DiscountAmount) AS adj
        WHERE a.Sex IN ('M', 'F')
          AND a.Weight IS NOT NULL
          AND a.TeethCount IS NOT NULL
          AND a.OrderId IS NOT NULL;

        SET @ProcessedCount = @@ROWCOUNT;

        -- Animais com sexo fora de M/F, ou sem peso, dentes ou pedido, não geram pagamento.
        SELECT @SkippedCount = COUNT(*) - @ProcessedCount FROM dbo.Animals;

        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0
            ROLLBACK TRANSACTION;

        THROW;
    END CATCH

    SELECT @ProcessedCount AS ProcessedAnimals, @SkippedCount AS SkippedAnimals;
END
GO
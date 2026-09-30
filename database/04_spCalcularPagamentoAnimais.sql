USE dbRecruta;
GO

CREATE OR ALTER PROCEDURE dbo.spCalcularPagamentoAnimais
     @ValorArrobaMacho   NUMERIC(18,2) = 100.00
    ,@ValorArrobaFemea   NUMERIC(18,2) = 50.00
    ,@PercentualAgio     NUMERIC(5,2)  = 10.00
    ,@PercentualDesagio  NUMERIC(5,2)  = 10.00
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    DECLARE @KgPorArroba    NUMERIC(5,2) = 15.00;
    DECLARE @DataCalculo    DATETIME2(0) = SYSDATETIME();
    DECLARE @MotivoAgio     VARCHAR(100) = CONCAT('Ágio de ', FORMAT(@PercentualAgio, '0.##'), '%: animal com 0 dentes');
    DECLARE @MotivoDesagio  VARCHAR(100) = CONCAT('Deságio de ', FORMAT(@PercentualDesagio, '0.##'), '%: animal com 6 ou mais dentes');
    DECLARE @QtdProcessados INT;

    BEGIN TRY
        BEGIN TRANSACTION;

        DELETE FROM dbo.tabPagamentoAnimais;

        INSERT INTO dbo.tabPagamentoAnimais
            (Animal, codPedido, Sexo, qtdDentes, Peso, QtdArrobas, ValorArrobaBase,
             ValorBase, ValorAgio, ValorDesagio, ValorPagar, TipoAjuste, MotivoAjuste, DataCalculo)
        SELECT
             a.Animal
            ,a.codPedido
            ,a.Sexo
            ,a.qtdDentes
            ,a.peso
            ,CAST(a.peso / @KgPorArroba AS NUMERIC(18,4))
            ,preco.ValorArrobaBase
            ,base.ValorBase
            ,ajuste.ValorAgio
            ,ajuste.ValorDesagio
            ,base.ValorBase + ajuste.ValorAgio - ajuste.ValorDesagio
            ,tipo.TipoAjuste
            ,CASE tipo.TipoAjuste
                 WHEN 'A' THEN @MotivoAgio
                 WHEN 'D' THEN @MotivoDesagio
                 ELSE CONCAT('Sem ágio ou deságio: animal com ', a.qtdDentes, ' dentes')
             END
            ,@DataCalculo
        FROM dbo.tabAnimais AS a
        CROSS APPLY (SELECT CASE a.Sexo WHEN 'M' THEN @ValorArrobaMacho
                                        ELSE @ValorArrobaFemea END AS ValorArrobaBase) AS preco
        CROSS APPLY (SELECT CAST(a.peso * preco.ValorArrobaBase / @KgPorArroba AS NUMERIC(18,2)) AS ValorBase) AS base
        CROSS APPLY (SELECT CASE WHEN a.qtdDentes = 0  THEN 'A'
                                 WHEN a.qtdDentes >= 6 THEN 'D'
                                 ELSE 'N' END AS TipoAjuste) AS tipo
        CROSS APPLY (SELECT CAST(CASE WHEN tipo.TipoAjuste = 'A'
                                      THEN base.ValorBase * @PercentualAgio / 100
                                      ELSE 0 END AS NUMERIC(18,2)) AS ValorAgio
                           ,CAST(CASE WHEN tipo.TipoAjuste = 'D'
                                      THEN base.ValorBase * @PercentualDesagio / 100
                                      ELSE 0 END AS NUMERIC(18,2)) AS ValorDesagio) AS ajuste
        WHERE a.Sexo IN ('M', 'F')
          AND a.peso IS NOT NULL
          AND a.qtdDentes IS NOT NULL;

        SET @QtdProcessados = @@ROWCOUNT;

        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0
            ROLLBACK TRANSACTION;

        THROW;
    END CATCH

    SELECT @QtdProcessados AS QtdAnimaisProcessados;
END
GO
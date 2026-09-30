USE dbRecruta;
GO

IF OBJECT_ID('dbo.AnimalPayments', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.AnimalPayments (
         AnimalId         INT            NOT NULL
        ,OrderId          INT            NOT NULL
        ,Sex              CHAR(1)        NOT NULL
        ,TeethCount       INT            NOT NULL
        ,Weight           NUMERIC(18,4)  NOT NULL
        ,Arrobas          NUMERIC(18,4)  NOT NULL
        ,BaseArrobaPrice  NUMERIC(18,2)  NOT NULL
        ,BaseAmount       NUMERIC(18,2)  NOT NULL
        ,PremiumAmount    NUMERIC(18,2)  NOT NULL CONSTRAINT DF_AnimalPayments_PremiumAmount  DEFAULT (0)
        ,DiscountAmount   NUMERIC(18,2)  NOT NULL CONSTRAINT DF_AnimalPayments_DiscountAmount DEFAULT (0)
        ,AmountToPay      NUMERIC(18,2)  NOT NULL
        ,AdjustmentType   CHAR(1)        NOT NULL
        ,AdjustmentReason VARCHAR(100)   NOT NULL
        ,CalculatedAt     DATETIME2(0)   NOT NULL CONSTRAINT DF_AnimalPayments_CalculatedAt   DEFAULT (SYSDATETIME())
        ,CONSTRAINT PK_AnimalPayments PRIMARY KEY (AnimalId)
        ,CONSTRAINT FK_AnimalPayments_Animals FOREIGN KEY (AnimalId) REFERENCES dbo.Animals (AnimalId)
        ,CONSTRAINT FK_AnimalPayments_Orders  FOREIGN KEY (OrderId)  REFERENCES dbo.Orders (OrderId)
        ,CONSTRAINT CK_AnimalPayments_AdjustmentType CHECK (AdjustmentType IN ('A', 'D', 'N'))
    );
END
GO
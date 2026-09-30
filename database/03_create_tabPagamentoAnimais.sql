USE dbRecruta;
GO

IF OBJECT_ID('dbo.tabPagamentoAnimais', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.tabPagamentoAnimais (
         Animal           INT            NOT NULL
        ,CodPedido        INT            NOT NULL
        ,Sexo             CHAR(1)        NOT NULL
        ,QtdDentes        INT            NOT NULL
        ,Peso             NUMERIC(18,4)  NOT NULL
        ,QtdArrobas       NUMERIC(18,4)  NOT NULL
        ,ValorArrobaBase  NUMERIC(18,2)  NOT NULL
        ,ValorBase        NUMERIC(18,2)  NOT NULL
        ,ValorAgio        NUMERIC(18,2)  NOT NULL CONSTRAINT DF_tabPagamentoAnimais_ValorAgio    DEFAULT (0)
        ,ValorDesagio     NUMERIC(18,2)  NOT NULL CONSTRAINT DF_tabPagamentoAnimais_ValorDesagio DEFAULT (0)
        ,ValorPagar       NUMERIC(18,2)  NOT NULL
        ,TipoAjuste       CHAR(1)        NOT NULL
        ,MotivoAjuste     VARCHAR(100)   NOT NULL
        ,DataCalculo      DATETIME2(0)   NOT NULL CONSTRAINT DF_tabPagamentoAnimais_DataCalculo  DEFAULT (SYSDATETIME())
        ,CONSTRAINT PK_tabPagamentoAnimais PRIMARY KEY (Animal)
        ,CONSTRAINT FK_tabPagamentoAnimais_tabAnimais FOREIGN KEY (Animal)    REFERENCES dbo.tabAnimais (Animal)
        ,CONSTRAINT FK_tabPagamentoAnimais_tabPedido  FOREIGN KEY (CodPedido) REFERENCES dbo.tabPedido (CodPedido)
        ,CONSTRAINT CK_tabPagamentoAnimais_TipoAjuste CHECK (TipoAjuste IN ('A', 'D', 'N'))
    );
END
GO
USE dbRecruta;
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_tabAnimais_tabPedido')
BEGIN
    ALTER TABLE dbo.tabAnimais
        ADD CONSTRAINT FK_tabAnimais_tabPedido
        FOREIGN KEY (codPedido) REFERENCES dbo.tabPedido (codPedido);
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes
               WHERE name = 'IX_tabAnimais_codPedido'
                 AND object_id = OBJECT_ID('dbo.tabAnimais'))
BEGIN
    CREATE INDEX IX_tabAnimais_codPedido ON dbo.tabAnimais (codPedido);
END
GO
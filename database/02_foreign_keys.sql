USE dbRecruta;
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_tabAnimais_tabPedido')
BEGIN
    ALTER TABLE dbo.tabAnimais
        ADD CONSTRAINT FK_tabAnimais_tabPedido
        FOREIGN KEY (CodPedido) REFERENCES dbo.tabPedido (CodPedido);
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes
               WHERE name = 'IX_tabAnimais_CodPedido'
                 AND object_id = OBJECT_ID('dbo.tabAnimais'))
BEGIN
    CREATE INDEX IX_tabAnimais_CodPedido ON dbo.tabAnimais (CodPedido);
END
GO
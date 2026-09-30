USE dbRecruta;
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_Animals_Orders')
BEGIN
    ALTER TABLE dbo.Animals
        ADD CONSTRAINT FK_Animals_Orders
        FOREIGN KEY (OrderId) REFERENCES dbo.Orders (OrderId);
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes
               WHERE name = 'IX_Animals_OrderId'
                 AND object_id = OBJECT_ID('dbo.Animals'))
BEGIN
    CREATE INDEX IX_Animals_OrderId ON dbo.Animals (OrderId);
END
GO
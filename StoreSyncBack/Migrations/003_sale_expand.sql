-- Migration 003: Expansão das tabelas de venda
-- Adiciona desconto, acréscimo e situação em sale e sale_item

ALTER TABLE sale ADD COLUMN IF NOT EXISTS discount DECIMAL(12,2) NOT NULL DEFAULT 0;
ALTER TABLE sale ADD COLUMN IF NOT EXISTS addition DECIMAL(12,2) NOT NULL DEFAULT 0;
ALTER TABLE sale ADD COLUMN IF NOT EXISTS status INT NOT NULL DEFAULT 1;

ALTER TABLE sale_item ADD COLUMN IF NOT EXISTS discount DECIMAL(12,2) NOT NULL DEFAULT 0;
ALTER TABLE sale_item ADD COLUMN IF NOT EXISTS addition DECIMAL(12,2) NOT NULL DEFAULT 0;

CREATE INDEX IF NOT EXISTS idx_sale_status ON sale(status);

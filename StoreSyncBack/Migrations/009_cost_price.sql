-- Migration 009: Adiciona preço de custo ao produto e snapshot no item de venda

ALTER TABLE product
    ADD COLUMN IF NOT EXISTS cost_price NUMERIC(12,2) NOT NULL DEFAULT 0;

ALTER TABLE sale_item
    ADD COLUMN IF NOT EXISTS cost_price NUMERIC(12,2) NOT NULL DEFAULT 0;

-- Migration 004: Adiciona coluna referencia auto-incrementada na tabela sale

CREATE SEQUENCE IF NOT EXISTS sale_referencia_seq START WITH 1 INCREMENT BY 1;

ALTER TABLE sale ADD COLUMN IF NOT EXISTS referencia VARCHAR(10);

UPDATE sale
SET referencia = LPAD(nextval('sale_referencia_seq')::TEXT, 5, '0')
WHERE referencia IS NULL;

ALTER TABLE sale ALTER COLUMN referencia SET NOT NULL;
ALTER TABLE sale ALTER COLUMN referencia SET DEFAULT LPAD(nextval('sale_referencia_seq')::TEXT, 5, '0');

CREATE UNIQUE INDEX IF NOT EXISTS idx_sale_referencia ON sale(referencia);

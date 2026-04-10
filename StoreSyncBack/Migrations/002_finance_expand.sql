-- Converte a coluna status de VARCHAR para INT
-- 1 = Aberto, 2 = Liquidado, 3 = Liquidado Parcialmente

-- Passo 1: remove o default existente (string 'pending') para não bloquear a conversão
ALTER TABLE finance ALTER COLUMN status DROP DEFAULT;

-- Passo 2: converte o tipo usando USING com mapeamento explícito
ALTER TABLE finance ALTER COLUMN status TYPE INT
    USING CASE
        WHEN status = 'paid'    THEN 2
        WHEN status = 'pending' THEN 1
        ELSE 1
    END;

-- Passo 3: define o novo default inteiro
ALTER TABLE finance ALTER COLUMN status SET DEFAULT 1;

-- Novas colunas
ALTER TABLE finance ADD COLUMN IF NOT EXISTS type         INT           NOT NULL DEFAULT 1;
ALTER TABLE finance ADD COLUMN IF NOT EXISTS title_type   INT           NOT NULL DEFAULT 1;
ALTER TABLE finance ADD COLUMN IF NOT EXISTS reference    VARCHAR(100);
ALTER TABLE finance ADD COLUMN IF NOT EXISTS settled_amount DECIMAL(12,2);
ALTER TABLE finance ADD COLUMN IF NOT EXISTS settled_at   DATE;
ALTER TABLE finance ADD COLUMN IF NOT EXISTS settled_note TEXT;
ALTER TABLE finance ADD COLUMN IF NOT EXISTS parent_id    UUID REFERENCES finance(finance_id);

-- Índices de suporte
CREATE INDEX IF NOT EXISTS idx_finance_type      ON finance(type);
CREATE INDEX IF NOT EXISTS idx_finance_status    ON finance(status);
CREATE INDEX IF NOT EXISTS idx_finance_parent_id ON finance(parent_id);

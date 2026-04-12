-- 005_commission_expand.sql
-- Expande a tabela commission: substitui "month" por período (start_date/end_date),
-- adiciona reference, observation e commission_rate (snapshot da taxa no momento da criação).

ALTER TABLE commission
    ADD COLUMN IF NOT EXISTS start_date DATE,
    ADD COLUMN IF NOT EXISTS end_date DATE,
    ADD COLUMN IF NOT EXISTS reference VARCHAR(50),
    ADD COLUMN IF NOT EXISTS observation TEXT,
    ADD COLUMN IF NOT EXISTS commission_rate DECIMAL(5,2);

-- Migrar dados existentes: mês único → período de 1 mês completo
UPDATE commission SET
    start_date = month,
    end_date = (month + INTERVAL '1 month - 1 day')::DATE
WHERE start_date IS NULL AND month IS NOT NULL;

-- Remover constraint antiga de unicidade (employee_id, month)
ALTER TABLE commission DROP CONSTRAINT IF EXISTS commission_employee_id_month_key;

-- Remover coluna antiga
ALTER TABLE commission DROP COLUMN IF EXISTS month;

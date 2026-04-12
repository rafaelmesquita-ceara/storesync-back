-- Tabela caixa
CREATE TABLE IF NOT EXISTS caixa (
    caixa_id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    referencia VARCHAR(100) NOT NULL,
    valor_abertura NUMERIC(18,2) NOT NULL DEFAULT 0,
    valor_fechamento NUMERIC(18,2),
    total_vendas NUMERIC(18,2) NOT NULL DEFAULT 0,
    total_sangrias NUMERIC(18,2) NOT NULL DEFAULT 0,
    total_suprimentos NUMERIC(18,2) NOT NULL DEFAULT 0,
    valor_faltante NUMERIC(18,2),
    valor_sobra NUMERIC(18,2),
    status INT NOT NULL DEFAULT 1,
    data_abertura TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    data_fechamento TIMESTAMP
);

-- Tabela movimentacao_caixa
CREATE TABLE IF NOT EXISTS movimentacao_caixa (
    movimentacao_caixa_id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    caixa_id UUID NOT NULL REFERENCES caixa(caixa_id),
    tipo INT NOT NULL,
    descricao VARCHAR(255),
    valor NUMERIC(18,2) NOT NULL,
    created_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP
);

-- Adicionar caixa_id à tabela sale (nullable para preservar dados existentes)
ALTER TABLE sale ADD COLUMN IF NOT EXISTS caixa_id UUID REFERENCES caixa(caixa_id);

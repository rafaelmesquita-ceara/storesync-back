-- Migration 006: Criação da tabela de clientes e vínculo com vendas

-- Tabela: client (clientes)
CREATE TABLE IF NOT EXISTS client (
    client_id   UUID        PRIMARY KEY DEFAULT gen_random_uuid(),
    reference   VARCHAR(10),
    name        VARCHAR(200) NOT NULL,
    cpf_cnpj    VARCHAR(18)  UNIQUE,
    phone       VARCHAR(20),
    email       VARCHAR(150) UNIQUE,
    address         VARCHAR(255),
    address_number  VARCHAR(20),
    address_complement VARCHAR(100),
    city        VARCHAR(100),
    state       CHAR(2),
    postal_code VARCHAR(10),
    status      INT          NOT NULL DEFAULT 1,
    created_at  TIMESTAMP    NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at  TIMESTAMP    NOT NULL DEFAULT CURRENT_TIMESTAMP
);

-- Índices
CREATE INDEX IF NOT EXISTS idx_client_cpf_cnpj  ON client(cpf_cnpj);
CREATE INDEX IF NOT EXISTS idx_client_email     ON client(email);
CREATE INDEX IF NOT EXISTS idx_client_phone     ON client(phone);
CREATE INDEX IF NOT EXISTS idx_client_status    ON client(status);
CREATE INDEX IF NOT EXISTS idx_client_created_at ON client(created_at);

-- Sequência para gerar reference no formato CLI + 5 dígitos
CREATE SEQUENCE IF NOT EXISTS client_reference_seq START 1;

ALTER TABLE client ALTER COLUMN reference SET DEFAULT 'CLI' || LPAD(nextval('client_reference_seq')::TEXT, 5, '0');

-- Vínculo opcional de cliente na venda
ALTER TABLE sale ADD COLUMN IF NOT EXISTS client_id UUID REFERENCES client(client_id) ON DELETE SET NULL;
CREATE INDEX IF NOT EXISTS idx_sale_client ON sale(client_id);

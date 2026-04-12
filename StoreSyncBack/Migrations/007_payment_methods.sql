-- Migration 007: Formas de pagamento e pagamentos de venda

CREATE TABLE IF NOT EXISTS payment_method (
    payment_method_id UUID         PRIMARY KEY DEFAULT gen_random_uuid(),
    name              VARCHAR(100) NOT NULL,
    type              INT          NOT NULL,  -- 1=Dinheiro, 2=DebitoCartao, 3=CreditoCartao, 4=Pix
    status            INT          NOT NULL DEFAULT 1,  -- 1=Ativo, 0=Inativo
    created_at        TIMESTAMP    NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at        TIMESTAMP    NOT NULL DEFAULT CURRENT_TIMESTAMP
);

CREATE INDEX IF NOT EXISTS idx_payment_method_type   ON payment_method(type);
CREATE INDEX IF NOT EXISTS idx_payment_method_status ON payment_method(status);

CREATE TABLE IF NOT EXISTS payment_method_rate (
    rate_id           UUID         PRIMARY KEY DEFAULT gen_random_uuid(),
    payment_method_id UUID         NOT NULL REFERENCES payment_method(payment_method_id) ON DELETE CASCADE,
    installments      INT          NOT NULL CHECK (installments >= 1),
    rate_percentage   NUMERIC(6,4) NOT NULL DEFAULT 0,
    created_at        TIMESTAMP    NOT NULL DEFAULT CURRENT_TIMESTAMP,
    UNIQUE (payment_method_id, installments)
);

CREATE INDEX IF NOT EXISTS idx_payment_method_rate_method ON payment_method_rate(payment_method_id);

CREATE TABLE IF NOT EXISTS sale_payment (
    sale_payment_id   UUID          PRIMARY KEY DEFAULT gen_random_uuid(),
    sale_id           UUID          NOT NULL REFERENCES sale(sale_id) ON DELETE CASCADE,
    payment_method_id UUID          NOT NULL REFERENCES payment_method(payment_method_id),
    amount            NUMERIC(12,2) NOT NULL CHECK (amount > 0),
    installments      INT           NOT NULL DEFAULT 1 CHECK (installments >= 1),
    surcharge_applied BOOLEAN       NOT NULL DEFAULT FALSE,
    surcharge_amount  NUMERIC(12,2) NOT NULL DEFAULT 0,
    created_at        TIMESTAMP     NOT NULL DEFAULT CURRENT_TIMESTAMP
);

CREATE INDEX IF NOT EXISTS idx_sale_payment_sale   ON sale_payment(sale_id);
CREATE INDEX IF NOT EXISTS idx_sale_payment_method ON sale_payment(payment_method_id);

ALTER TABLE sale ADD COLUMN IF NOT EXISTS troco NUMERIC(12,2) NOT NULL DEFAULT 0;

-- Migration 000: Criação do schema inicial do StoreSyncBack
-- Cria todas as tabelas do sistema e a tabela de controle de migrations

-- Habilita extensão para UUID se não estiver habilitada
CREATE EXTENSION IF NOT EXISTS "uuid-ossp";

-- Tabela: employee (funcionários)
CREATE TABLE IF NOT EXISTS employee (
    employee_id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    name VARCHAR(200) NOT NULL,
    cpf VARCHAR(14) UNIQUE NOT NULL,
    role VARCHAR(50) NOT NULL DEFAULT 'user',
    commission_rate DECIMAL(10,2) DEFAULT 0,
    created_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP
);

-- Tabela: "user" (usuários de login) - nome entre aspas pois é palavra reservada do PostgreSQL
CREATE TABLE IF NOT EXISTS "user" (
    user_id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    login VARCHAR(100) UNIQUE NOT NULL,
    password VARCHAR(255) NOT NULL,
    employee_id UUID NOT NULL REFERENCES employee(employee_id) ON DELETE CASCADE,
    created_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP
);

-- Tabela: category (categorias de produtos)
CREATE TABLE IF NOT EXISTS category (
    category_id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    name VARCHAR(100) NOT NULL UNIQUE,
    created_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP
);

-- Tabela: product (produtos)
CREATE TABLE IF NOT EXISTS product (
    product_id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    reference VARCHAR(50) UNIQUE NOT NULL,
    name VARCHAR(200) NOT NULL,
    category_id UUID REFERENCES category(category_id) ON DELETE SET NULL,
    price DECIMAL(12,2) NOT NULL DEFAULT 0,
    stock_quantity INTEGER NOT NULL DEFAULT 0,
    created_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP
);

-- Tabela: sale (vendas)
CREATE TABLE IF NOT EXISTS sale (
    sale_id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    employee_id UUID NOT NULL REFERENCES employee(employee_id),
    total_amount DECIMAL(12,2) NOT NULL DEFAULT 0,
    sale_date TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    created_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP
);

-- Tabela: sale_item (itens de venda)
CREATE TABLE IF NOT EXISTS sale_item (
    sale_item_id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    sale_id UUID NOT NULL REFERENCES sale(sale_id) ON DELETE CASCADE,
    product_id UUID NOT NULL REFERENCES product(product_id),
    quantity INTEGER NOT NULL DEFAULT 1,
    total_price DECIMAL(12,2) NOT NULL DEFAULT 0,
    created_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP
);

-- Tabela: commission (comissões de funcionários)
CREATE TABLE IF NOT EXISTS commission (
    commission_id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    employee_id UUID NOT NULL REFERENCES employee(employee_id) ON DELETE CASCADE,
    month DATE NOT NULL,
    total_sales DECIMAL(12,2) NOT NULL DEFAULT 0,
    commission_value DECIMAL(12,2) NOT NULL DEFAULT 0,
    created_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    UNIQUE(employee_id, month)
);

-- Tabela: finance (registros financeiros)
CREATE TABLE IF NOT EXISTS finance (
    finance_id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    description TEXT NOT NULL,
    amount DECIMAL(12,2) NOT NULL DEFAULT 0,
    due_date DATE NOT NULL,
    status VARCHAR(50) NOT NULL DEFAULT 'pending',
    created_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP
);

-- Tabela de controle de migrations (criada por último)
CREATE TABLE IF NOT EXISTS historico_versao (
    id SERIAL PRIMARY KEY,
    numero_release VARCHAR(20) NOT NULL UNIQUE,
    data_atualizacao TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP
);

-- Índices opcionais para melhorar performance em consultas comuns
CREATE INDEX IF NOT EXISTS idx_employee_cpf ON employee(cpf);
CREATE INDEX IF NOT EXISTS idx_user_login ON "user"(login);
CREATE INDEX IF NOT EXISTS idx_product_category ON product(category_id);
CREATE INDEX IF NOT EXISTS idx_sale_employee ON sale(employee_id);
CREATE INDEX IF NOT EXISTS idx_sale_item_sale ON sale_item(sale_id);
CREATE INDEX IF NOT EXISTS idx_sale_item_product ON sale_item(product_id);
CREATE INDEX IF NOT EXISTS idx_commission_employee ON commission(employee_id);
CREATE INDEX IF NOT EXISTS idx_commission_month ON commission(month);
CREATE INDEX IF NOT EXISTS idx_finance_due_date ON finance(due_date);

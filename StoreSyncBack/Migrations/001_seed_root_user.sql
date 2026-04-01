-- Migration 001: Seed do usuário root/admin
-- Cria um funcionário admin e um usuário de login com senha "admin"
-- Senha hash: BCrypt de "admin"

DO $$
DECLARE
    rootId UUID;
BEGIN
    -- Verifica se já existe usuário admin
    IF NOT EXISTS (SELECT 1 FROM "user" WHERE login = 'admin') THEN
        -- Cria o funcionário root
        INSERT INTO employee (employee_id, name, cpf, role, commission_rate)
        VALUES (gen_random_uuid(), 'root', '00000000000', 'admin', 0)
        RETURNING employee_id INTO rootId;

        -- Cria o usuário admin vinculado ao funcionário root
        -- Senha: "admin" (hash BCrypt: $2a$11$FANxhhoSr97Yytr8FYaI8.3L50sY9Dnz1TjfyTY.oX9CNWSzsbuiO)
        INSERT INTO "user" (user_id, login, password, employee_id)
        VALUES (gen_random_uuid(), 'admin', '$2a$11$FANxhhoSr97Yytr8FYaI8.3L50sY9Dnz1TjfyTY.oX9CNWSzsbuiO', rootId);
    END IF;
END
$$;

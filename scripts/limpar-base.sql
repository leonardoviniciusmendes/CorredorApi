-- Limpa os dados de dominio da base corretorAPI.
-- Nao remove a tabela __EFMigrationsHistory.
-- Execute somente no banco correto.

SET FOREIGN_KEY_CHECKS = 0;

TRUNCATE TABLE `PosContrato`;
TRUNCATE TABLE `Contrato`;
TRUNCATE TABLE `Dependente`;
TRUNCATE TABLE `Endereco`;
TRUNCATE TABLE `Cliente`;
TRUNCATE TABLE `PessoaFisica`;
TRUNCATE TABLE `PessoaJuridica`;
TRUNCATE TABLE `Documento`;
TRUNCATE TABLE `Simulacao`;
TRUNCATE TABLE `FaixaEtaria`;
TRUNCATE TABLE `Historico`;
TRUNCATE TABLE `Lead`;

-- Opcional: descomente para remover tambem os scripts seedados.
-- TRUNCATE TABLE `Script`;

SET FOREIGN_KEY_CHECKS = 1;

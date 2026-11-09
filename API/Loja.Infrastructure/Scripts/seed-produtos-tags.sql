-- Dados demonstrativos para Tags, Produtos, TagsProdutos e ProdutoImagens.
-- Execute uma vez em um banco com as migrations aplicadas.

START TRANSACTION;

INSERT INTO `Tags` (`Tipo`, `Descricao`, `Cor`) VALUES ('Categoria', 'Camisetas', '#6C5CE7');
SET @tag_categoria_camisetas = LAST_INSERT_ID();

INSERT INTO `Produtos` (`Titulo`, `DescricaoBasica`, `Descricao`, `Valor`, `Ativo`, `DataCadastro`, `UsuarioId`)
VALUES ('Camiseta Essencial Branca', 'Camiseta básica de algodão.', 'Camiseta confortável de algodão, ideal para o uso diário.', 59.90, 1, NOW(6), @usuario_id);
SET @produto_1 = LAST_INSERT_ID();


INSERT INTO `TagsProdutos` (`ProdutoId`, `TagId`) VALUES
(@produto_6, @tag_categoria_casacos), (@produto_6, @tag_genero_unissex), (@produto_6, @tag_cor_preto), (@produto_6, @tag_tamanho_g);

INSERT INTO `ProdutoImagens` (`ProdutoId`, `ImagemUrl`, `Referencia`) VALUES
(@produto_1, 'https://picsum.photos/seed/loja-camiseta-branca/800/1000', 1),

COMMIT;

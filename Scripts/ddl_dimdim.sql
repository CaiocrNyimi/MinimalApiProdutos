CREATE TABLE Categorias (
    Id INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    Nome NVARCHAR(100) NOT NULL
);

CREATE TABLE Produtos (
    Id INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    Nome NVARCHAR(150) NOT NULL,
    Preco DECIMAL(18,2) NOT NULL,
    Estoque INT NOT NULL,
    CategoriaId INT NOT NULL,
    CONSTRAINT FK_Produtos_Categorias_CategoriaId FOREIGN KEY (CategoriaId)
        REFERENCES Categorias(Id)
        ON DELETE CASCADE
);

CREATE INDEX IX_Produtos_CategoriaId ON Produtos(CategoriaId);
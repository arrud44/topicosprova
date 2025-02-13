CREATE DATABASE LojaDB;
USE LojaDB;

CREATE TABLE Clientes (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    Nome VARCHAR(100) NOT NULL
);

CREATE TABLE Servicos (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    Nome VARCHAR(100) NOT NULL,
    Preco DECIMAL(10, 2) NOT NULL,
    Status BOOLEAN NOT NULL
);

CREATE TABLE Contratos (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    ClienteId INT,
    ServicoId INT,
    PrecoCobrado DECIMAL(10, 2) NOT NULL,
    DataContratacao DATETIME NOT NULL,
    FOREIGN KEY (ClienteId) REFERENCES Clientes(Id),
    FOREIGN KEY (ServicoId) REFERENCES Servicos(Id)
);

DROP TABLE LojaDB;

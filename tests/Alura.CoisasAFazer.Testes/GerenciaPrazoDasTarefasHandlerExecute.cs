﻿using Alura.CoisasAFazer.Core.Commands;
using Alura.CoisasAFazer.Core.Models;
using Alura.CoisasAFazer.Infrastructure;
using Alura.CoisasAFazer.Services.Handlers;
using Microsoft.EntityFrameworkCore;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace Alura.CoisasAFazer.Testes
{
    public class GerenciaPrazoDasTarefasHandlerExecute
    {
        [Fact]
        public void QuandoTarefasEstiveremAtrasadasDeveMudarSeuStatus()
        {
            //arrange
            var compCateg = new Categoria(1, "Compras");
            var casaCateg = new Categoria(2, "Casa");
            var trabCateg = new Categoria(3, "Trabalho");
            var saudCateg = new Categoria(4, "Saúde");
            var higiCateg = new Categoria(5, "Higiene");

            var tarefas = new List<Tarefa>
            {
                //atrasadas a partir de 1/1/2019
                new Tarefa(1, "Tirar lixo", casaCateg, new DateTime(2018, 12, 31), null, StatusTarefa.Criada),
                new Tarefa(4, "Fazer o almoço", casaCateg, new DateTime(2017, 12, 01), null, StatusTarefa.Criada),
                new Tarefa(9, "Ir à academia", saudCateg, new DateTime(2018, 12, 31), null, StatusTarefa.Criada),
                new Tarefa(7, "Concluir o relatório", trabCateg, new DateTime(2018, 05, 07), null, StatusTarefa.Pendente),
                new Tarefa(10, "Beber água", saudCateg, new DateTime(2018, 12, 31), null, StatusTarefa.Criada),
                //dentro do prazo em 1/1/2019
                new Tarefa(8, "Comparecer à reunião", trabCateg, new DateTime(2018, 11, 12), new DateTime(2018, 11, 30), StatusTarefa.Concluida),
                new Tarefa(2, "Arrumar a cama", casaCateg, new DateTime(2019, 04, 05), null, StatusTarefa.Criada),
                new Tarefa(3, "Escovar os dentes", higiCateg, new DateTime(2019, 01, 02), null, StatusTarefa.Criada),
                new Tarefa(5, "Comprar presente pro João", compCateg, new DateTime(2019, 10, 08), null, StatusTarefa.Criada),
                new Tarefa(6, "Comprar ração", compCateg, new DateTime(2019, 11, 20), null, StatusTarefa.Criada)
            };
            var options = new DbContextOptionsBuilder<DbTarefasContext>()
                .UseInMemoryDatabase("DbTarefasContext")
                .Options;
            var contexto = new DbTarefasContext(options);
            var repo = new RepositorioTarefa(contexto);

            repo.IncluirTarefas(tarefas.ToArray());

            var comando = new GerenciaPrazoDasTarefas(new DateTime(2019, 01, 01));
            var handler = new GerenciaPrazoDasTarefasHandler(repo);

            //act
            handler.Execute(comando);

            //assert
            var tarefasEmAtraso = repo.ObtemTarefas(t => t.Status == StatusTarefa.EmAtraso);
            Assert.Equal(5, tarefasEmAtraso.Count());
        }

        [Fact]
        public void QuandoInvocadoDeveChamarAtulizarTarefasNaQtdeVezesDoTotalDeTarefasAtrasadas()
        {
            //arrange
            var categ = new Categoria("Dummy");
            var tarefas = new List<Tarefa>
            {
                //atrasadas a partir de 1/1/2019
                new Tarefa(100, "Tirar lixo", categ, new DateTime(2018, 12, 31), null, StatusTarefa.Criada),
                new Tarefa(104, "Fazer o almoço", categ, new DateTime(2017, 12, 01), null, StatusTarefa.Criada),
                new Tarefa(109, "Ir à academia", categ, new DateTime(2018, 12, 31), null, StatusTarefa.Criada)
            };
            var mock = new Mock<IRepositorioTarefas>();
            mock.Setup(r => r.ObtemTarefas(It.IsAny<Func<Tarefa, bool>>()))
                .Returns(tarefas);
            var repo = mock.Object;

            var comando = new GerenciaPrazoDasTarefas(new DateTime(2019, 1, 1));
            var handler = new GerenciaPrazoDasTarefasHandler(repo);

            //act
            handler.Execute(comando);

            //assert
            mock.Verify(r => r.AtualizarTarefas(It.IsAny<Tarefa[]>()), Times.Once());
        }
    }
}

using Alura.CoisasAFazer.Core.Commands;
using Alura.CoisasAFazer.Core.Models;
using Alura.CoisasAFazer.Infrastructure;
using Alura.CoisasAFazer.Services.Handlers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Linq;
using Xunit;

namespace Alura.CoisasAFazer.Testes
{
    public class CadastraTarefaHandlerExecute
    {
        [Fact]
        public void DadaTarefaComInfoValidasDeveIncluirNoBD()
        {
            // arrange
            var comando = new CadastraTarefa("Estudar Xunit", new Categoria("Estudo"), new DateTime(2019, 12, 31));
            var options = new DbContextOptionsBuilder<DbTarefasContext>().UseInMemoryDatabase("DbTarefasContext").Options;
            var contexto = new DbTarefasContext(options);
            var repo = new RepositorioTarefa(contexto);
            var mock = new Mock<ILogger<CadastraTarefaHandler>>();
            var handler = new CadastraTarefaHandler(repo, mock.Object);

            //act
            handler.Execute(comando);

            //assert
            var tarefa = repo.ObtemTarefas(t => t.Titulo == "Estudar Xunit").FirstOrDefault();
            Assert.NotNull(tarefa);
        }

        [Fact]
        public void DataTarefaComInfoValidaDeveLogar()
        {
            var tituloTarefaEsperado = "Usar Moq para aprofundar conhecimento de API";
            var comando = new CadastraTarefa(tituloTarefaEsperado, new Categoria("Estudo"), new DateTime(2019, 12, 31));
            var mockLogger = new Mock<ILogger<CadastraTarefaHandler>>();
            var mock = new Mock<IRepositorioTarefas>();

            LogLevel levelCapturada = LogLevel.Error;
            object stateCapturada;
            Exception exceptionCapturada;
            string mensagemCapturada = string.Empty;

            mockLogger.Setup(l =>
                l.Log(
                    It.IsAny<LogLevel>(),
                    It.IsAny<EventId>(),
                    It.IsAny<object>(),
                    It.IsAny<Exception>(),
                    (Func<object, Exception, string>)It.IsAny<object>()))
                .Callback<IInvocation>(invocation =>
                {
                    var formatador = invocation.Arguments[4];
                    var invokeFormatador = formatador.GetType().GetMethod("Invoke");

                    levelCapturada = (LogLevel)invocation.Arguments[0];
                    stateCapturada = invocation.Arguments[2];
                    exceptionCapturada = (Exception)invocation.Arguments[3];
                    mensagemCapturada = (string)invokeFormatador?.Invoke(formatador, new[] { stateCapturada, exceptionCapturada });
                });

            var handler = new CadastraTarefaHandler(mock.Object, mockLogger.Object);

            //act
            handler.Execute(comando);

            //assert
            Assert.Equal(LogLevel.Debug, levelCapturada);
            Assert.Contains(tituloTarefaEsperado, mensagemCapturada);

        }

        [Fact]
        public void QuandoExceptionForLancadaResultadoIsSuccessDeveSerFalse()
        {
            //arrange
            var comando = new CadastraTarefa("Estudar Xunit", new Categoria("Estudo"), new DateTime(2019, 12, 31));
            var mock = new Mock<IRepositorioTarefas>();
            var mockLogger = new Mock<ILogger<CadastraTarefaHandler>>();
            var repo = mock.Object;

            mock.Setup(r => r.IncluirTarefas(It.IsAny<Tarefa[]>()))
                .Throws(new Exception("Houve um erro na inclusão de tarefas"));

            var handler = new CadastraTarefaHandler(repo, mockLogger.Object);

            //act
            var resultado = handler.Execute(comando);

            //assert
            Assert.False(resultado.IsSuccess);
        }

        [Fact]
        public void QuandoExceptionForLancadaDeveLogarAMensagemDaExcecao()
        {
            //arrange
            var mensagemDeErroEsperada = "Houve um erro na inclusão de tarefas";
            var excecaoEsperada = new Exception(mensagemDeErroEsperada);

            var comando = new CadastraTarefa("Estudar Xunit", new Categoria("Estudo"), new DateTime(2019, 12, 31));
            var mock = new Mock<IRepositorioTarefas>();
            var mockLogger = new Mock<ILogger<CadastraTarefaHandler>>();
            var repo = mock.Object;

            mock.Setup(r => r.IncluirTarefas(It.IsAny<Tarefa[]>()))
                .Throws(excecaoEsperada);

            var handler = new CadastraTarefaHandler(repo, mockLogger.Object);
            
            //act
            var resultado = handler.Execute(comando);

            //assert
            mockLogger.Verify(l =>
                l.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<object>((v, t) => true),
                    excecaoEsperada,
                    It.Is<Func<object, Exception, string>>((v, t) => true)),
                Times.Once());

            // abordagem alternativa para verificação
            //mockLogger.Verify(l =>
            //    l.Log(
            //        LogLevel.Error,
            //        It.IsAny<EventId>(),
            //        It.IsAny<object>(),
            //        excecaoEsperada,
            //        (Func<object, Exception, string>)It.IsAny<object>()),
            //    Times.Once());
        }
    }
}

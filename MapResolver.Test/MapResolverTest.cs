namespace MapperResolver.Test
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class MapperResolverTest
    {
        [TestMethod]
        public void Simple_resolver()
        {
            Pessoa pessoa = new Pessoa() { Codigo = 1, Endereco = new Endereco() { Bairro = "Industrial" } };

            DTODePessoa dto = new DTODePessoa();

            MapResolver.Instance.Map<Pessoa, DTODePessoa>(pessoa, dto);

            Assert.AreEqual(1, dto.Codigo);
            Assert.AreEqual("Industrial", dto.Endereco_Bairro);
            Assert.IsNull(dto.Endereco_Rua);
        }
    }

    public class Endereco
    {
        public string Bairro { get; set; }

        public string Rua { get; set; }
    }

    public class Pessoa
    {
        public int Codigo { get; set; }

        public Endereco Endereco { get; set; }
    }

    public class DTODePessoa
    {
        public int Codigo { get; set; }

        public string Endereco_Bairro { get; set; }

        public string Endereco_Rua { get; set; }
    }
}

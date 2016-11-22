# MapResolver
Resolve map properties automatically

## Usage  
```csharp

public class Pessoa
{
    public int Codigo { get; set; }

    public Endereco Endereco { get; set; }
}

public class Endereco
{
    public string Bairro { get; set; }

    public string Rua { get; set; }
}

public class DTODePessoa
{
    public int Codigo { get; set; }

    public string Endereco_Bairro { get; set; }

    public string Endereco_Rua { get; set; }
}

Pessoa pessoa = new Pessoa() { Codigo = 1, Endereco = new Endereco() { Bairro = "Industrial" } };

DTODePessoa dto = new DTODePessoa();

MapResolver.Instance.Map<Pessoa, DTODePessoa>(pessoa, dto);

```

# Enunciado do Trabalho 2

**Data limite de entrega: ~~27 de Abril~~ 3 de Maio**

**Objectivos**: _meta-programação_ -- análise e manipulação programática de código
intermédio com API de `System.Reflection.Emit`.

No seguimento do Trabalho 1 desenvolvido na biblioteca **Webao** pretende-se
desenvolver uma nova classe `WebaoDynBuilder` com o mesmo comportamento de
`WebaoBuilder`, mas que pretende **simplificar** a definição e obter
**melhor desempenho** dos _web access objects_.

Para tal os _web access objects_ passam a poder ser definidos por interfaces, conforme o exemplo seguinte:

<table>
<tr>
<td>

```csharp
Request req = new HttpRequest();
WebaoDynArtist webaoDyn = 
  (WebaoDynArtist) WebaoDynBuilder
    .Build(typeof(WebaoDynArtist), req);
```


</td>
<td>

```csharp
[BaseUrl("http://ws.audioscrobbler.com/2.0/")]
[AddParameter("format", "json")]
[AddParameter("api_key", "************")]
public interface WebaoDynArtist
{
    [Get("?method=artist.getinfo&artist={name}")]
    [Mapping(typeof(DtoArtist), ".Artist")]
    Artist GetInfo(string name);


    [Get("?method=artist.search&artist={name}&page={page}")]
    [Mapping(typeof(DtoSearch), ".Results.ArtistMatches.Artist")]
    List<Artist> Search(string name, int page);
}
```

</td>
</tr>
</table>

`WebaoDynBuilder` deve gerar em tempo de execução (dinamicamente)
uma implementação de uma classe que implementa a interface especificada.
**ATENÇÃO** a classe gerada dinamicamente:
1. **NÃO** pode fazer operações de reflexão. 
2. **NÃO** estende de `AbstractAccessObject`.

Por exemplo, a obtenção das propriedades `Results`, `ArtistMatches` e `Artist`
que é feita para o método `Search()` por via de reflexão em
`AbstractAccessObject` será feita com acesso directo às respectivas propriedades
em código IL gerado dinamicamente por via de `System.Reflection.Emit`.

**APENAS** será usada reflexão na própria classe `WebaoDynBuilder` e suas auxiliares e
**NÃO** nas classes que este gera dinamicamente.

Todos os testes unitários elaborados no Trabalho 1 devem ter uma versão correspondente
para _web access objects_ gerados dinamicamente com `WebaoDynBuilder`.

## Parte 0

Certifique-se que no Trabalho 1 implementou pelo menos um caso de um _web access
object_ com um método que receba mais que um parâmetro e incluindo de tipo
valor, como o método `Search()` exemplificado na introdução a este trabalho.

Caso não o tenha feito, então adicione um método nessas condições.

## Parte 1

**ATENÇÃO**: A _meta-programação_ requer abordagens diferentes no desenvolvimento de software.
Repare que vai ter necessidade de fazer _debug_ NÃO apenas do código que escreve
mas também de código que foi gerado dinamicamente.
Para tal, o processo de desenvolvimento de _meta-programação_ deve ser rigoroso
e cauteloso, recomendando-se que cumpra a abordagem descrita.

Como suporte ao desenvolvimento de `WebaoDynBuilder` deve usar as ferramentas:
  * `ildasm`
  * `peverify`

Em cada etapa deve desenvolver em C# uma classe _dummy_ num projecto à parte com
uma implementação semelhante àquela que pretende que seja gerada através da API
de `System.Reflection.Emit`. 
Compile a classe _dummy_ e use a ferramenta `ildasm` para visualizar as instruções
IL que servem de base ao que será emitido através da API de `System.Reflection.Emit`. 

Recomenda-se que implemente a geração dinâmica dos métodos de _web access objects_
incrementalmente pela seguinte ordem, verificando em cada etapa que os métodos
retornam o valor correcto:
1. Retornar apenas o URL do pedido a realizar com base nos parâmetros que foram
     passados ao método. Ou seja, para o exemplo da introdução verificar numa
     primeira etapa que a instância gerada de `WebaoDynArtist` na chamada de
     `Search("muse", 1)`, retorna
     `http://ws.audioscrobbler.com/2.0/?method=artist.search&artist=muse&page=1&format=json&api_key=************`
2. Retornar apenas o DTO correspondente. Ou seja, para o exemplo da introdução
     que a instância gerada de `WebaoDynArtist` na chamada de `Search("muse",
     1)`, retorna uma instância válida de `DtoSearch`
3. Retornar a instância de `List<Artist>`.

Exemplo da classe _dummy_ em cada etapa:

<table>
<tr>
<td>

```csharp
class WebaoDummy {
  public object Search(object name, int page) {
    String path = "?method=artist.search&artist={name}&page={page}";
    path = path.Replace("{name}", name.ToString());
    path = path.Replace("{page}", page.ToString());
    return path;
  }
}
```

</td>
<td>

```csharp
class WebaoDummy {
  public object Search(object name, int page) {
    // 1. processamento do path...
    DtoSearch dto = (DtoSearch) req.Get(path, typeof(DtoSearch));
    return dto;
  }
}
```

</td>
<td>

```csharp
class WebaoDummy {
  public List<Artist> Search(object name, int page) {
    // 1. processamento do path...
    // 2. obter dto
    return dto.Results.ArtistMatches.Artist;
  }
}
```

</td>
</tr>
</table>

**ATENÇÃO** que o código que implementar no `WebaoDynBuilder` não deve ser _hard
coded_ mas sim com base na informação anotada nos respectivos _custom
attributes_.
Ou seja, o `path`, o tipo do DTO e as propriedade acedidas são as que tiverem
especificadas nos _custom attributes_

Para cada uma das etapas anteriores deve realizar **TODOS** os passos seguintes
sem saltar nenhum deles.

* a) implementar a classe _dummy_ em c# com o código equivalente ao que pretende
  gerar.
* b) compilar essa classe com `csc \optimize+ \debug-`
* c) visualizar com o `ildasm` as instruções IL correspondentes e implementar a
  geração dinâmica dessas instruções via `Emit`. **ATENÇÃO** esta implementação
  **NÃO** deve estar comprometida com nenhum caso concreto de `Artist`, `Team`, etc.
* d) executar o `Build()` do `WebaoDynBuilder` e gravar a dll com a classe
  gerada dinamicamente.
* e) verificar a correcção da dll gerada através da ferramenta `peverify`.
* f) executar os métodos do _web access object_ gerado e verificar que retorna o
valor esperado na respectiva etapa 1, 2 ou 3. 

**REPITA** todos as etapas e passos para um _web access object_ que lide com 
tipos valor, e.g. `DtoGeoTopTracks`.

Use sempre a ferramenta `peverify` para despistar os erros/excepções ocorridas
na utilização das classes geradas.
Grave sempre as DLLs com as classes geradas pelo `WebaoDynBuilder` e valide
através da ferramenta `peverify` a correcção do código IL.

## Parte 2

Adicione à solução um novo projecto de uma aplicação consola (.Net Framework)
designado  **WebaoBench** que compare o desempenho entre as implementações dos 
Trabalhos 1 e 2.

Na medição de desempenho não deve ser realizado IO e nem conversão de JSON em objectos.
Para tal use uma versão _mock_ de `IRequest` que tenha o menor processamento possível
na execução do método `Get()`.
O método `Get()` deste _mock_ deve obter os objectos duma _cache_ interna em memória.

Para as medições de desempenho **use a abordagem apresentada nas aulas**
(**atenção que testes de desempenho NÃO são testes unitários**). Registe e
comente os desempenhos obtidos entre as duas abordagens. 

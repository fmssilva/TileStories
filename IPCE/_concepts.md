# em vez de guia de acompanhamento ter "ficheiros de materia"?? exemplo ciclos e ifs, funçoes e recursividade... etfc...??? 
assim os docs das aulas já servem para guia de acompanhamento??? 

# `que exercicios fazer eu à frente, que exercicios fazer em dialogo passo a passo a perguntar-lhes como fazer e depis fazer para assim ter class a participar ... e depois exercicios pra eles fazer sozinhos e eu mstro soluçao no fim...`
`no inicio da aula deixar uns minutos para perguntar duvidas sobre ultima aula ou revisoes ou duvidas...`

# ver qd falar de AI- usem com cuidado. tentem fazer primeiro. dp de 10  minutos a sofrer nao conseguem entao sim perguntem ao gpt, copiem e colem no claude e peçam analise critica... e comando nao é pra fazr mas sim pra fazer e ensinar... e dar mais exemplos dp... e peguem no teste do ano anterior e peçam mais exercicios parecidos

# aula 2 ??
## funçoes recursivas
- dar exemplos de programaçao dinamica simples e interessantes , ou quick sort ou coisas assim para verem potencial.. e para verem o "click mental que têm de dar para conseguir dividir o problema em partes e pensar: se uma parte for resolvida eu consigo pegar nisso e resolver a parte acima? se sim, então é programaºao dinamica e podemos usar recursividade. 
- e na pratica para traduzir formulas matematicas, é só traduzir mesmo a forma diretamente. nem é peciso  pensar em mais nada. dar mais alguns exempl
- pensar os casos base, e depois construir daí

- funções tambem ajudam para perceber o código. dando um bom nome à função, nós podemos praticamente implementar um pseudo code bem crobusto e limpoe depois é facil implementar. exemplo: 
register_user(id, name, email) -> str: 
    if not (is_free_id() and is_free_email()): 
        return "id or email not free"
    if not (is_email_valid())
        return "email is not valid! must have @ etc..."
    add_user_to_DB(id,name,email)
    return "user registered with success"
» e assim ao pensar assim em "high level, nomes de funçoes" conseguimos implementar um codigo limpo e bem organizado e depois dá para testar localmenete cada funçao auxiliar a ver se tudo funciona bem...

- para tal é importante dar bons nomes!!!! nomes que explicam logo o que a função faz


# ciclos

# if
- dar exemplos de if, elif, else
- dar exemplos em que ordem importa - o if de cima não pode apanhar o if de baixo
- exemplos de confusao com and vs or
- dizer se tem de haver parentesis?     if not cat1 > 0 and cat2 > 0: ??


# aulas futuras...
Cada função deve desempenhar uma tarefa bem definida, deve ter um nome bem escolhido que sugira a sua tarefa, e não deve haver mais nenhuma função que se intrometa nessa tarefa.
» falar nas boas praticas de nomes das coisas
» falar em efeitos laterais em funções que se devem evitar
» falar e demonstrar eexmplos diferentes de pass by value e pass by reference??? 
» falar sobre variavis globais ou locais... quais as regras pra funções e loops... 


» sobre variaveis... são dinamicas... elas apontam para objeto que especifica typo de dados e dados... e então podemos ter 
i = 2  # i aponta para int
e mais à frente no codigo ter 
i = "2" # i aponta para uma string
e não há problema... em linguagens "estáticas"??? haveria como em java??? porque são compiladas e tem de bater logo tudo certo?? 
python é interpretado...??? 


print » dizer que com f podemos ter expessões dentro de {} e não apenas variavis... e podemos formatar diferentes tipos de dados e output etc..


# clean code:
## naming cnventions:  
- nomes de funçõs e variavis » snake?? year_lenght
- nomes de constantes: CAPSLOCK
- nomes representativos da responsabilidade da função/variavel para ser facil de ler e entender
- nomes de boolean sere pergunta?? 

## explicar codigo
- no código escrever os tipos de dados de arg, retorno, variavis etc... não é validado pelo interpretador, mas é para nós perecebermos melhor o codigo, exemplo no projeto
- ter inline comments lines... e tb """ """ comments no inicio das funççoes com resumo geral da funçao... 
- ter codigo organizado por funçoes para ser fácil de perceber... e ter main com IO vs resto

## Separação de Responsabilidades e Refactoring 
- separar interaçao com user numa função ou fichiro (pensar tipo frontend - o website)... da lógica (pensar que pode ser o banckend um servidor a correr a logica...) (embora na pratica pode ser apenas uma simples app tudo junto, mas é sempre bom separar bem as coisas para fácil de manter e alterar..)
» prints lá no meio do código só para testes. não é para entregar no projeto... 

- pensar o refactoring (quando usamos mesma logica em vários locais » extrair para função que faz isso... assim é facil corrigir codigo só num sitio... podemos fazer testes locais )

# testes
- forma simples de testar é simplesmente meter um main em cada ficheiro que implementam funções e esse main enchem-no com testes a fazer para testar a função uma por uma para garantir que faz o que querem






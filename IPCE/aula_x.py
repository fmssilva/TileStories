# %%
"""
A.4) [EXPLAIN] [6 min -> 10:40] — Boas Práticas - bons nomes às funções e variáveis

    Uma forma boa de programar é escrever as ações que queremos fazer,
    e converter cada uma dessas frases/ações no nome de uma função.
    No exemplo anterior, dava para escrever o main passo a passo de
    forma intuitiva:
        "Dado o total, quantas notas de 20 cabem?"
            »» num_notas(total, 20)
        "Depois quanto sobra?" »» resto(total, 20)
        "Pego nesse resto e faço a mesma coisa para notas de 10"
        "E o resto final fica para moedas de 1"

    É uma forma muito simples e prática de fazer um programa: dividi-lo
    em funções que são as ações que temos de fazer na prática.

    Se as funções tiverem nomes bem escolhidos, escrever código fica
    quase como falar português (ou inglês).

    Outro exemplo, ainda mais natural de pensar um programa. 
    Exemplo para registar um utilizador, 
    podemos não saber os pormenores de como implementar uma função, 
    mas já sabemos que é isto que precisa de acontecer:
        
    TODO: onde variaveis sao criadas
"""
# pensamos as ações de alto nível que temos que fazer:
def register_user(id, name, email) -> str:
    a = 20
    if not is_email_valid(email):
        return "email inválido"
    if not (is_free_id(id) and is_free_email(email)):
        return "id ou email já usados"
    add_user_to_db(id, name, email, a)
    return "utilizador registado com sucesso"

# para já podemos deixar estas funções "stub" (esqueleto), com um valor
# fixo de retorno ou só "pass", assim já temos um programa que corre
# de ponta a ponta, mesmo sem termos implementado a lógica a sério ainda

# E é bom pensar-se também o tipo de retorno que queremos dar 
# para depois bater tudo certo 
def is_free_id(id: str) -> bool:
    # TODO
    return True

def is_free_email(email: str) -> bool:
    #TODO
    return True

def is_email_valid(email: str) -> bool:
    # TODO
    return True

def add_user_to_db(id: str, name: str, email: str) -> None:
    # TODO
    pass

"""
    Dividindo o programa em funções com nomes claros e bem escolhidos:
    - é fácil pensar o programa
    - é fácil outra pessoa ler e perceber o código (ex: o professor a corrigir)
    - poupa em comentários, porque os nomes já são autoexplicativos
    - permite dividir tarefas e testar cada função à parte
        » depois do grupo decidir um bom "register_user" geral, cada um
        pode implementar e testar a sua função sem depender das outras
"""






# %%
"""
===========================================================================
FECHO [7 min -> 13:00]
===========================================================================

[BOA PRÁTICA DE USO DE IA - (Claude, GPT, Gemini, etc.)]

Nesta, e nas demais cadeiras, a IA tem muitas vantagens 
SE usada corretamente — PARA ENSINAR E NÃO PARA FAZER!

Boa prática:
- Tenta sempre sozinho primeiro. 
    Aceita os erros e problemas que surgem de bom grado, 
    explora e testa diferentes opções... 
    Mesmo que demore, é isso que te ensina a programar.
  
- Se ao fim de 10 minutos a sofrer não tens uma solução,
    pede à IA para resolver, 
    
    MAS nunca peças simplesmente: "resolve-me isto".

    PEDE SEMPRE PRÁ IA ENSINAR - exemplo de prompt: 

        [problema...]

        Dá-me a solução para o problema,

        /// bloco de prompt reutilizavel para todos os pedidos: ///
        
        ---
        MAS EXPLICA TODOS OS DETALHES IMPORTANTES QUE EU DEVIA SABER
        para passar num teste difícil de Introdução à Programação do meu mestrado,
        e no geral para ser bom programador ciente das boas práticas a seguir.

        E depois dá-me também mais alguns exemplos práticos de exercícios
        semelhantes que podem sair no teste e respetiva solução e explicação.

        Escreve de forma simples e natural e de forma concisa,
        MAS garante que me explicas todos os conceitos e gotchas
        relacionados com este tipo de problemas que eu deva ficar a saber
        e que pode sair no teste.
        ---
    
        »» Podes guardar este prompt, 
            e ir melhorando com o tempo 
            e adaptando para cada cadeira, 
            para ser fácil copiar e colar em cada chat,
            para ter respostas boas sem estar sempre a escrever a mesma coisa. 
            Assim poupas tempo, e garantes que usas sempre um bom prompt
            que faz com que a IA te ensine, e não apenas fazer por ti. 

- Quando fizeres um exercício por ti, na tua versão, 
    Mete na IA e pede uma análise crítica. 
    exemplo de prompt:

       [Exercicio e proposta de solução]

       Faz uma análise crítica da minha solução
       Diz-me o que está errado, o que pode melhorar...

       /// aqui o mesmo bloco de prompt reutilizavel para todos os pedidos: ///

- Pegar também nos testes dos anos anteriores e pedir à IA
        
        [Teste em anexo...]

        Dá-me mais 3 exercícios parecidos com o exercício X deste teste,
        com dificuldade semelhante e crescente.

        /// aqui o mesmo bloco de prompt reutilizavel para todos os pedidos: ///
                  
- Tendo um bom prompt que vão melhorando com o tempo, 
    conseguem meter a IA a servir de professor e realmente ajudar a aprender mais rapido. 
    MAS só funciona se usares a IA para ensinar, e não para fazer por ti.
    Tens que continuar a correr o código, a mudar o código, a experimentar com o código.
    Se usas a IA para fazer os exercícios e trabalhos por ti, 
    no teste (sem consulta!) ninguém te vai safar.
"""
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






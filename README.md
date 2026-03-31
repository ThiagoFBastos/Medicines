<h1 align="center" style="font-weight: bold;">Medicines 💊</h1>

<p align="center">
 <a href="#tech">Technologies</a> • 
 <a href="#started">Getting Started</a> • 
 <a href="#contribute">Contribute</a>
</p>

<p align="center">
    <b>Medicines is a project aimed at helping users manage their medications efficiently. It provides features to track quantities, schedule dosage times, view registered medicines, and easily add or remove them.</b>
</p>

<h2 id="tech">💻 Technologies</h2>

- .NET 8
- Docker
- Docker Compose
- Postgres
- Worker Class
- telegrambots
- Entity Framework Core


<h2 id="started">🚀 Getting started</h2>

<h3> Commands </h3>
- <b>/start [username]</b> - Inicia a interação com o bot e registra o usuário com o nome de usuário especificado. <br>
- <b>/add [remédio] [quantidade] [horário] </b> - Adiciona um remédio com a quantidade especificada e um horário para tomar. <br>
- <b>/remove [remédio]</b> - Remove um remédio da lista. <br>
- <b>/lookup [remédio]</b> - Procura por um remédio específico. <br>
- <b>/list</b> - Exibe a lista de remédios. <br>
- <b>/help</b> - Exibe esta mensagem de ajuda. <br>
<br>
Obs: the bot talks with the user in portuguese

<h3>Prerequisites</h3>

- .NET 8
- Docker
- Telegram Bot Token

<h3>Cloning</h3>

How to clone your project

```bash
git clone https://github.com/ThiagoFBastos/Medicines.git
```

<h3>Config .env variables</h2>

Use the `.env-example` as reference to create your configuration file `.env` with your Credentials

<h3>Starting</h3>

How to start your project

```bash
docker compose up --build
```

<h2 id="contribute">📫 Contribute</h2>

Here you will explain how other developers can contribute to your project. For example, explaining how can create their branches, which patterns to follow and how to open an pull request

1. `git clone https://github.com/ThiagoFBastos/Medicines.git`
2. `git checkout -b feature/NAME`
3. Follow commit patterns
4. Open a Pull Request explaining the problem solved or feature made, if exists, append screenshot of visual modifications and wait for the review!

<h3>Documentations that might help</h3>

[📝 How to create a Pull Request](https://www.atlassian.com/br/git/tutorials/making-a-pull-request)

[💾 Commit pattern](https://gist.github.com/joshbuchea/6f47e86d2510bce28f8e7f42ae84c716)
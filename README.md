# Дашборд графика строительства

## Front-end

Фронт имеет несколько страниц - компоненты страниц можно изменить в домалки и наоборот. 

### `/`

Главная страница дашборда. 

Диаграмму Гантта можно складывать по веткам, можно фильтровать по поиску. 

Через меню можно открыть админ панель. 

### `/admin`

При создании загруженные данные пополняются в базу данных на выбранную дату. 
При редактировании данные заменяют существующие данные на дату. 

### `/diff`

График сравнения сроков по датам

### `/table`

Отдельный дашборд стройобъектов, каждый объект имеет модал с графиком строительства вект данного объекта

## GraphQL API

### `availableDates`

Даты в базе данных

```graphql
availableDates: [DateTime!]!
```

### `scheduleObjects`

Объекты для главного дашборда

```graphql
scheduleObjects(date: DateTime!): [ScheduleObject!]! @cost(weight: "10")
```

### `dateDiff`

Объекты сравнения по 2 датам

```graphql
dateDiff(oldDate: DateTime!, newDate: DateTime!): [ScheduleDiff!]!
```

### `scheduleSubtrees`

Функция для таблицы для получения веток по кодам объектов

```graphql
scheduleSubtrees(date: DateTime!, codes: [String!]!): [[ScheduleObject!]!]!
```

## Как запустить

```bash
cd board
npm install
npm run dev
```
```bash
cd Server
dotnet run
```

В [`DbCommon.cs`](./Server/Common/DbCommon.cs) строка подключения к БД, по умолчанию ожидает Postgres на `localhost:5432` пользователь `postgres`, пароль `postgres`, созданная база данных `CREATE DATABASE postgres`

# Разработка сервиса бронирования билетов на мероприятие

Зависимости:
- Visual Studio 2026
- .NET 10
- ASP.NET Core MVC
- Swagger

Данный сервис получает на вход мероприятия, которые пользователь может получать(GET), создавать(POST), обновлять(PUT) и удалять(DELETE), и выводить информацию в виде HTTP-ответов с телом ответа.

Всего реализовано 5 методов:
1. Получить все мероприятия (HTTPGET())
2. Получить мероприятие по его ID (HTTPGET("{id}"))
3. Создать мероприятие по телу запроса(HTTPPOST())
4. Обновить мероприятие в теле запроса(HTTPPUT())
5. Удалить мероприятие по ID(HTTPDELETE())

## Сборка и запуск сервиса
Чтобы запустить данный сервис можно воспользоваться 2 способами: через IDE(Visual Studio) и через командную строку(CMD)
1. Через IDE(Visual Studio)

Запустить можно через кнопку F5 или нажать на панели https (Проект сразу соберется и запустится).
<img width="1453" height="795" alt="image" src="https://github.com/user-attachments/assets/8a754c84-37c0-4c6f-9fa8-e0b106e84d0e" />
2. Через командную строку(CMD)

Заходим в папку с проектом, где находится файл EventTicketBookingService.csproj через командную строку и запускаем команды

1) dotnet build
2) dotnet run --environment Development
3) start https://localhost:7246/swagger

## Unit-тесты
Чтобы запустить unit-тесты можно воспользоваться 2 способами: через IDE(Visual Studio) и через командную строку(CMD)
1. Через IDE(Visual Studio)

Нажать на проект с юнит-тестами ПКМ и нажать на "выполнить тесты"
<img width="1308" height="833" alt="image" src="https://github.com/user-attachments/assets/3929f8b1-9b47-4d88-90ce-449dd7fc46d4" />

2. Через командную строку(CMD)

Заходим в папку с проектом, где находится файл EventService.csproj через командную строку и запускаем команды

1) dotnet build
2) dotnet test 

### Swagger
Данный инструмент предназначен для тестирования работы HTTP-методов, прописанных в контроллере ASP.NET Core.
Запустится swagger в браузере(Edge по умолчанию), где будут отображены все доступные HTTP-методы
<img width="1915" height="1025" alt="image" src="https://github.com/user-attachments/assets/c3a4639b-d908-409f-ae76-28e521849fc9" />

## Тест данных методов
В браузере в swagger будут доступны 5 HTTP-методов, выполняющих тесты запросов.
Для выполнения запросов, необходимо выбрать один из методов, отображенных в swagger и нажать Try it out, после Execute для выполнения самого запроса.


1. При вызове **GET /Events**. Получаем все мероприятия в формате JSON-объекта(Id, Title, Description, StartAt, EndAt).
   
Была добавлена возможность фильтрации по подстроке в Title(регистронезависимый, частичное совпадение), а также по from и to (from  - события, которые начинаются не раньше указанной даты, to - события, которые заканчиваются не позже указанной даты)

В случае если нет никаких мероприятий, то выведется пустое тело в виде "{
  "totalCount": 0,
  "events": [],
  "pageIndex": 1,
  "pageSizeByIndex": 0
}" (см. 1 скриншот). С внедрением возможности фильтрации и пагинации, данный метод теперь может получать нужные названия и регулировать диапазон времени от начала события и до конца события, а также выводить мероприятия по нужной странице с определенным количеством элементов на странице(см 2 скриншот). Иначе после добавления(POST смотреть во 2 пункте) или изменения(PUT смотреть в 4 пункте) будет отображены JSON-объекты(см. 3 скриншот)

1 Скриншот
<img width="1411" height="731" alt="image" src="https://github.com/user-attachments/assets/5968d97a-2147-422d-9a8c-a4b6cb0815bb" />

2 Скриншот
<img width="1405" height="666" alt="image" src="https://github.com/user-attachments/assets/eadc8739-ee7d-4163-b318-14e032947799" />

3 Скриншот
<img width="1411" height="707" alt="image" src="https://github.com/user-attachments/assets/a75843e4-ae57-484b-8c77-74f2f5d42b87" />


2. при вызове **POST /Events**. Создаем мероприятия в теле запроса.
Создается новый ID автоматически. Title должен быть заполнен, Description необязательное поле для заполнения. StartAt и EndAt в формате DateTime. EndAt должен быть позже StatAt.

Ошибки:
* ID обязательное для заполнения
* Title обязательное для заполнения
* StartAt обязательное для заполнения
* EndAt обязательное для заполнения
* Конец мероприятия должен быть позже начала мероприятия
Пример POST-запроса
<img width="1466" height="913" alt="image" src="https://github.com/user-attachments/assets/f23e8623-6cb4-4827-99c5-91b9aa78276d" />
Пример ошибки POST-запроса
<img width="1419" height="567" alt="image" src="https://github.com/user-attachments/assets/adce673f-1b88-4d33-b477-c68dd2ae8258" />

3. При вызове **GET /Events/{id}**. Получаем мероприятие по ID. В случае неккоректного ID будет выведено сообщение "Не найдено событие по ID: {id}"
Пример правильного ID
<img width="1425" height="796" alt="image" src="https://github.com/user-attachments/assets/8c1ae10b-80b2-4cfd-ac81-2ab24348d3e4" />
Пример несуществующего ID
<img width="1415" height="854" alt="image" src="https://github.com/user-attachments/assets/7736a8c0-b5af-45fc-8a8a-ce5e0f2170f5" />

4. При вызове **PUT /Events/{id}**. Изменяет мероприятие по ID и его телу запроса, ID не поменяется, если попробовать внести изменения в теле запроса. Title должен быть заполнен, Description необязательное поле для заполнения. StartAt и EndAt в формате DateTime. EndAt должен быть позже StatAt.
Пример PUT-запроса
<img width="1474" height="801" alt="image" src="https://github.com/user-attachments/assets/77f63213-6245-48c3-a8dd-1b7fe83a4869" />
<img width="1403" height="745" alt="image" src="https://github.com/user-attachments/assets/fe5cb9e3-073b-4003-9823-b4988126b942" />

5. При вызове **DELETE /Events/{id}**. Удаляет полностью мероприятие по ID.

Пример при правильном ID
<img width="1409" height="836" alt="image" src="https://github.com/user-attachments/assets/f31b0028-db4d-46f8-b56d-69df74c589f1" />
Пример при несуществующем ID
<img width="1432" height="861" alt="image" src="https://github.com/user-attachments/assets/9d769df8-b545-4c81-8e70-37361d5b38f7" />

## Общий формат ошибок, пойманный глобальным обработчиком ошибок
{

  "type": null,
  
  "title": null,
  
  "status": {statusCode}, 404(NotFound), 400(BadRequest), 500(InternalServer)
  
  "detail": {Описание ошибки},
  
  "instance": null,
  
  "extensions": {}
  
}


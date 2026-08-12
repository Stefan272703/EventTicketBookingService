# Разработка сервиса бронирования билетов на мероприятие

Зависимости:
- Visual Studio 2026
- .NET 10
- ASP.NET Core MVC
- Swagger

Данный сервис получает на вход мероприятия, которые пользователь может получать(GET), создавать(POST), обновлять(PUT) и удалять(DELETE), и выводить информацию в виде HTTP-ответов с телом ответа.

Всего реализовано 7 методов:
1. Получить все мероприятия (HTTP GET /Events)
2. Получить мероприятие по его ID (HTTP GET /Events/{id})
3. Создать мероприятие по телу запроса (HTTP POST /Events)
4. Обновить мероприятие в теле запроса по ID (HTTP PUT /Events/{id})
5. Удалить мероприятие по ID (HTTP DELETE /Events/{id})
6. Создать бронь конкретного мероприятия по ID (HTTP POST /Events/{id}/book)
7. Получить бронь по его ID (HTPP GET /Bookings/{id})

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

Нажать на решение, ПКМ и нажать на "выполнить тесты"
<img width="1496" height="785" alt="image" src="https://github.com/user-attachments/assets/35f97517-7479-4464-86b3-cfdc5467936f" />


2. Через командную строку(CMD)

Заходим в папку с решением, где находится файл EventTicketBookingService.slnx, через командную строку запускаем команды

1) dotnet build
2) dotnet test 

### Swagger
Данный инструмент предназначен для тестирования работы HTTP-методов, прописанных в контроллере ASP.NET Core.
Запустится swagger в браузере(Edge по умолчанию), где будут отображены все доступные HTTP-методы
<img width="1486" height="742" alt="image" src="https://github.com/user-attachments/assets/a232fbad-8da6-4259-98a5-4b88da62849b" />


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

6. При вызове **POST /Events/{id}/book**. Создает новую бронь мероприятия по его ID.

Сначала создается мероприятие согласно 2 пункту, по которому делается с методом **POST /Events**.

Затем в **POST /Events/{id}/book** создаем бронь по ID мероприятию, который создали недавно.

Пример при правильном ID.
<img width="1428" height="867" alt="image" src="https://github.com/user-attachments/assets/c231eb3a-527c-45b6-a06f-90bd507626d3" />

Пример при несуществующем ID.
<img width="1428" height="867" alt="image" src="https://github.com/user-attachments/assets/11639c15-316d-4428-b827-5f52baed6926" />

При создании брони, необходимо некоторое время, чтобы получить новый статус и дату, когда обработка прошла успешно. 

Необходимо, чтобы брони, которые могут быть более 1 или 1000, не "забивали" поток и обрабатывались на фоне.

Для этого был реализован фоновый процесс, в который передается бронь в рамках очереди и обрабатывает бронь и переводит его из статуса Pending в статус Confirmed или Rejected.

Имитация операции всего 2 секунды для одной брони.

Сценарий получения статуса брони рассматривается в 7 пункте по **GET /Bookings/{id}**.

7. При вызове **GET /Bookings/{id}**. Получаем бронь по его ID.
При создании брони согласно 7 пункту с методом **POST /Events/{id}/book**, можно получить его объект модели Booking, формат которого
```
{
  "id": int,
  "eventId": int,
  "status": BookingStatus,
  "createdAt": DateTime,
  "processedAt": DateTime
}
где
id - целочисленный тип, является идентификатором самой брони и является уникальным
eventId - целочисленный тип, является идентификатором самого мероприятия 
status - тип перечисления BookingStatus, принимает 1 из 3 состояний (Pending, Confirmed или Rejected) в числовом формате
createdAt - дата с момента создания брони
processedAt - дата завершения обработки брони
```

Пример при правильном ID и до того, как обработается бронь (Имеет статус Pending, т.е. 0 и не существует время заершения обработки)
<img width="1429" height="873" alt="image" src="https://github.com/user-attachments/assets/645c27a8-206b-4df3-99e8-24f4b88a3dc0" />
Пример при правильном ID и после того, как обработается бронь (Имеет статус Confirmed, т.е. 1 и существует время заершения обработки)
<img width="1436" height="877" alt="image" src="https://github.com/user-attachments/assets/f59ef7a4-38be-4344-9bbe-cc448b105cf9" />
Пример при несуществующем ID
<img width="1429" height="904" alt="image" src="https://github.com/user-attachments/assets/41161e31-3ae1-4d42-ad0c-3f900503dbcb" />


## Общий формат ошибок, пойманный глобальным обработчиком ошибок
{

  "type": null,
  
  "title": null,
  
  "status": {statusCode}, 404(NotFound), 400(BadRequest), 500(InternalServer)
  
  "detail": {Описание ошибки},
  
  "instance": null,
  
  "extensions": {}
  
}


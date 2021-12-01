using System.Net;
using System.Text.Json;
using my_db;
using System.Text.RegularExpressions;

HttpListener listener = new HttpListener(); //создаём экземпляр класса HttpListener и обзываем его listener
// установка адресов прослушки
listener.Prefixes.Add("http://localhost:5000/banks/");
listener.Start();
Console.WriteLine("Ожидание подключений...");
bool flag = true;
while (flag == true)
{
    // метод GetContext блокирует текущий поток, ожидая получение запроса 
    HttpListenerContext context = listener.GetContext();
    HttpListenerRequest request = context.Request;
    string? URLstring = Convert.ToString(request.Url);
    // вытаскиваем IFSC из URL
    string? IFSCstring = null;
    IFSCstring = URLstring.Replace("http://localhost:5000/banks/", "");
    string? reply = null; // сюда мы будем выплёвывать инфу по нашему банку в виде строки (ну или инфу по ошибке/завершению)
    string pattern = @"\w{11}";

    if (IFSCstring == "LET-ME-OUT-") // это уже баловство) Если отправить вместо IFSC сочетание "LET-ME-OUT-", то сервер прекратит прослушку
    {
        flag = false;
        reply = "You have just requested the sever to stop. No more requests until you start it again.";
    }
    else
    {

        if (Regex.IsMatch(IFSCstring, pattern))
        {
            Bank? bank1 = null; //здесь будем хранить экземпляр с нашим банком, специально созданный класс с набором атрибутов из namespace "my_db"

            // для начала поищем банк в локальной базе
            using (ApplicationContext db = new ApplicationContext())
            {
                db.Database.EnsureCreated();
                Bank? bank = db.Banks.Find(IFSCstring); // попробуем выбрать элемент с IFSC, который у нас запросили

                if (bank != null)
                {
                    // мы нашли банк в локальной базе, запишем инфу в созданный ранее экземпляр
                    bank1 = bank;
                }
            }

            // если в локальной базе не нашли, обращаемся к публичному API
            if (bank1 == null)
            {
                Console.WriteLine("Мы не нашли банк в локальной базе. Придётся обращаться к публичному API");

                HttpClient client = new HttpClient();
                string requesttorazorpay_string = "https://ifsc.razorpay.com/" + IFSCstring; //строка requesttorazorpay_stringb хранит адрес, по которому мы сделаем запрос на публичный API
                string responseBody = await client.GetStringAsync(requesttorazorpay_string); //используем метод GetStringAsync чтобы получить в строку responsebody JSON, отдаваемый публичным API
                
                bank1 = JsonSerializer.Deserialize<Bank>(responseBody);

                using (ApplicationContext db = new ApplicationContext())
                {
                    db.Banks.Add(bank1);
                    db.SaveChanges();
                    Console.WriteLine("Банк успешно сохранён");

                }
            }

            // тем или иным путём мы получили экземпляр банка, теперь преобразуем его в строку JSON
            reply = JsonSerializer.Serialize<Bank>(bank1);
        }
        else
        {
            reply = "You have requested no data, URL should be something like http://localhost:5000/banks/XXXXXXXXXXX";
        }
    }
    // получаем объект ответа
    HttpListenerResponse response = context.Response;
    // создаем ответ в виде кода html
    response.ContentType = "application/json";
    byte[] buffer = System.Text.Encoding.UTF8.GetBytes(reply);
    // получаем поток ответа и пишем в него ответ
    response.ContentLength64 = buffer.Length;
    Stream output = response.OutputStream;
    output.Write(buffer, 0, buffer.Length);

    // закрываем поток
    output.Close();
}

// останавливаем прослушивание подключений
listener.Stop();
Console.WriteLine("Обработка подключений завершена");
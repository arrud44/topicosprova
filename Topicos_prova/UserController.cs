namespace prova.Controllers
{
    public class UserController
    {
        public bool Login(String email, String password)
        {
            // Implementação simples de login
            return email == "abc@gmail.com" && password == "password";
        }
    }
}

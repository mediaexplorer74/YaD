using Newtonsoft.Json;

namespace Ya.D.Models
{
    public class Feedback
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public string Theme { get; set; }
        public string Email { get; set; }
        public string Text { get; set; }
        public byte[] AppID { get; set; }

        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
}

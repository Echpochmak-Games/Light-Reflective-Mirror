
internal struct Bearer
{
    public string Name { get; set; }
    public string Token { get; set; }

    public Bearer(string name, string token)
    {
        Name = name;
        Token = token;
    }
}

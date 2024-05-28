using Pishock_API_Test;

string api = "6745e755-3a8c-454a-a4fd-d7a3e37f0dc9";
string username = "MarcelOwO";
string shareCode = "26AE5AF3A77";

var test = new PiShock(username, api, shareCode);

await test.shock(10,1);
await test.vibe(25, 5);
await test.beep(1);
await test.shock(25, 1);



while (true) ;



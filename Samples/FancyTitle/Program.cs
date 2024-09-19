using MiniFB;
using System;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

AppDomain.CurrentDomain.UnhandledException += static (_, e) => Console.Error.WriteLine(e.ExceptionObject?.ToString());

const string titlePrefix = "Fancy Title ";
const int titleMarqueeRate = 30;

uint width = 800, height = 600, size = width * height;
uint noise, carry, seed = 0xbeef;
uint iteration = 0;

using var window = OperatingSystem.IsWindows()
    ? new Window(titlePrefix, width, height, WindowFlags.Resizable, smallIcon: new(icon16(), 16, 16), bigIcon: new(icon32(), 32, 32))
    : new Window(titlePrefix, width, height, WindowFlags.Resizable);

var buffer = new Argb[size];

void resize(Window window, int newWidth, int newHeight)
{
    width = (uint)newWidth;
    height = (uint)newHeight;

    // It's not safe to resize the buffer while it's pinned. That would be case if, for example, 'window.LifetimeState is WindowLifetimeState.UpdatingWithFixedBuffer'.
    // Instead we signalize the need to resize the buffer by setting 'size' and do the resize at the begin of the next frame.
    //
    // Array.Resize(ref buffer, newWidth * newHeight);

    size = width * height;
}

window.Resize += resize;

window.TrySetViewport(50, 50, width - 50 - 50, height - 50 - 50);
resize(window, (int)width - 100, (int)height - 100); // to resize buffer

using var timer = new Timer();
var avg = new BinomialMovingAverageFilter<double>(30);

do
{
    if (buffer.Length < size)
    {
        Array.Resize(ref buffer, (int)size);
    }

    for (var i = 0; i < size; ++i)
    {
        noise = seed;
        noise >>= 3;
        noise ^= seed;
        carry = noise & 1;
        noise >>= 1;
        seed >>= 1;
        seed |= carry << 30;
        noise &= 0xff;
        buffer[i] = new(0xff, (byte)noise, (byte)noise, (byte)noise);
    }

    if (OperatingSystem.IsWindows() || OperatingSystem.IsLinux())
    {
        window.Title = $"{window.Title.AsSpan()[..titlePrefix.Length] switch
        {
        [var head, .. var tail] when (iteration = (iteration + 1) % titleMarqueeRate) is 0 => $"{tail}{head}",
            var title => title
        }} - {avg.Advance(1 / timer.Delta):0.00} frame/s";
    }
}
while (
    window.Update(buffer, width, height) is UpdateState.Ok
    && window.WaitForSync()
);

/* *** */

static ReadOnlySpan<Argb> icon16() => MemoryMarshal.Cast<char, Argb>(
      "\0\0āਁ⠨焨祹쉹节繾ｾ蚆ﾆ貌ﾌ貌ﾌ祹ｹ繾ｾ筻祹쉹⨪焪āਁ\0\0āਁ歫\ud86b뢸ﾸ뾿\uffbf"
    + "뾿\uffbf뾿\uffbf뾿\uffbf뾿\uffbf뚶ﾶ呔ｔ뚶ﾶ屜｜뢸ﾸ龟ﾟ浭\ud86dāਁ㠸蔸뢸ﾸ뾿"
    + "\uffbf뎳ﾳꦩﾩꦩﾩꞧﾧꎣﾣꎣﾣ隖ﾖꦩﾩ讋ﾋ견ﾬ兑ｑ뚶ﾶ㠸蔸筻챻뾿\uffbf閑ﾐ墕｟冫｠冨｣斣ｦ"
    + "멳ｨ멱ｫ녫ｪ莎ｱ厤ｺ綦ﾑ뷀\uffbf뾿\uffbf筻챻繾ﹾ鞓ﾑ펄ｚ䧎｢䗃｣䗁ｧ暻ｬ\uda79ｮ\uda76"
    + "ｳ쵬ｰ邟ｼ䖺ﾇ䧂ﾔ櫊ﾨ쯌ￌ繾ﹾ繾ｾ궇｣䧐｢䟈･䎽･趡･조､푴ｯ푱ｴ掰ｿ䎴ﾃ䎴ﾉ䞼ﾕ흤ﾊ킔ﾧ繾ｾ蚆ﾆ"
    + "띾ｒ䗄｡䎽､䂳､蚘､빨｢쥫ｬ쥩ｱ徦ｼ䂪ﾁ䂪ﾆ䎱ﾒ쭝ﾆ쑢ﾋ螇ﾇ貌ﾌ钛ｗ䗂･䎻ｧ禟､龄､뵥･졩ｯꂑｻ"
    + "厨ﾀ㾨ﾄ审ﾇ躒ﾌ쩚ﾈ셓ﾆ讋ﾋ醑ﾑ䊻｟䛃ｪ䎻ｬꚂ､뱥､빤ｨ쩨ｳ䖹ﾉ䆭ﾆ䂩ﾉ疘ﾉ왙ﾄ챙ﾍ쉒ﾊ薅ﾅ躎ﾎ䛅"
    + "ｩ䫎ｵ䣆ｸ낈ｭ읩ｭ쩨ｲ홬ｾ䧃ﾖ䖶ﾓ䒲ﾖ粠ﾕ퉜ﾐ\ud95cﾙ칔ﾕ罿ｿ繾ｾ壁ｷ䯎ｼ푲ｰ쩪ｮ졨ｱ쁶ｹ䫅ﾖ䫃ﾝ"
    + "䖶ﾙꊈﾏ쩙ﾊ푚ﾕ\udb5aﾞ沪ﾲ繾ｾ邐ﺐ疯ﾆ䛁ｹ읪ｭ빢ｫ뵟ｮ땭ｶ䖸ﾒ䖶ﾘ䆪ﾕ饿ﾊ빑ﾅ읓ﾏ칒ﾘ莟ﾨ杧"
    + "\ufe67龟첟랽ﾸ꒖ｷ쑦ｮ뭟ｬꙶｴ纕ﾁ䒳ﾔ䒲ﾚ銄ﾋ굤ﾆ뭎ﾆ쑏ﾐ鎑ﾫꎤﾧ⤩착䩊蕊틒ￒ얶ﾶ캜ﾡ쎉ﾒꂒﾌ殯ﾗ"
    + "沶ﾡ沵ﾥ뙽ﾔ뱱ﾓ쎂ﾡ캕ﾱ馨ﾫ摤､༏蔏āਁ硸\ud878늲ﾲ香ﾙ邐ﾐ邐ﾐ邐ﾐ邐ﾐ邐ﾐ邐ﾐ邐ﾐ辏ﾏ祹ｹ恠｠ⴭ"
    + "\ud82dāਁ\0\0āਁ⌣焣嵝쉝噖卓ｓ奙ｙ屜｜屜｜奙ｙ卓ｓ噖䥉쉉ጓ焓āਁ\0\0"
    );

static ReadOnlySpan<Argb> icon32() => MemoryMarshal.Cast<char, Argb>(
      "\0\0\0\0\0\0\0\0\0\0\0ἀ\0瀀\0ꈀ\0찀\0픀\0\uff00\0\uff00\0\uff00\0\uff00"
    + "\0\uff00\0\uff00\0\uff00\0\uff00\0\uff00\0\uff00\0\uff00\0\uff00\0픀\0"
    + "찀\0ꈀ\0瀀\0ἀ\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0ⴀ\0숀㨺Ｚ罿ｿ鮛ﾛ궭ﾭ놱ﾱ뾿"
    + "\uffbf뾿\uffbf뾿\uffbf뾿\uffbf뾿\uffbf뾿\uffbf뾿\uffbf뾿\uffbf꺮ﾮ견ﾬ뾿"
    + "\uffbf뾿\uffbfꪪﾪꆡﾡ鮛ﾛ肀ﾀ㨺Ｚ\0숀\0ⴀ\0\0\0\0\0\0\0\0\0\0\0攀⤩骚ﾚ뾿"
    + "\uffbf뾿\uffbf뾿\uffbf뾿\uffbf뾿\uffbf뾿\uffbf뾿\uffbf뾿\uffbf뾿\uffbf뾿"
    + "\uffbf뾿\uffbf뾿\uffbf뎳ﾳ橪ｪ肀ﾀ쏃ￃ뚶ﾶ畵ｵ誊ﾊ쇁\uffc1뾿\uffbf뾿\uffbf骚ﾚ⠨"
    + "\0昀\0\0\0\0\0\0\0㌀㈲גּ뒴ﾴ뾿\uffbf뾿\uffbf뾿\uffbf뾿\uffbf뾿\uffbf뾿"
    + "\uffbf뾿\uffbf뾿\uffbf뾿\uffbf뾿\uffbf뾿\uffbf뾿\uffbf뾿\uffbf늲ﾲฎ．\0"
    + "\uff00题ﾘ궭ﾭ☦Ｆᰜ＜ꊢﾢ늲ﾲ罿ｿ题ﾘ랷ﾷ㌳דּ\0㌀\0\0\0\0\0툀鲜ﾜ뾿\uffbf뾿\uffbf"
    + "뾿\uffbf뾿\uffbf뾿\uffbf뾿\uffbf뾿\uffbf뾿\uffbf뾿\uffbf뾿\uffbf뾿\uffbf"
    + "뾿\uffbf뾿\uffbf뾿\uffbf뾿\uffbfꖥﾥ辏ﾏ벼ﾼ뾿\uffbf躎ﾎ睷ｷ뒴ﾴ鲜ﾜ㸾＾噖ｖ샀"
    + "\uffc0鲜ﾜ\0팀\0\0\0㐀偐ｐ뾿\uffbf뾿\uffbf뾿\uffbf뾿\uffbf떵ﾵ鶝ﾝ邐ﾐ辏ﾏ肀ﾀ"
    + "肀ﾀ肀ﾀ肀ﾀ肀ﾀ肀ﾀ肀ﾀ肀ﾀ肀ﾀ肀ﾀ肀ﾀ肀ﾀ辏ﾏ鞗ﾗ궭ﾭ놱ﾱ剒ｒ剒ｒ뒴ﾴ뾿\uffbf偐ｐ\0㐀\0"
    + "蔀覉ﾉ뾿\uffbf뾿\uffbf벼ﾼ邐ﾐ桲ｩ檨ｱ抛ｩ棊ｸ廁ｱ嚾ｭ惞ｾ膔､ｹ쭼ｯ쭺ｱｿ깫ｧﾄ앳ｴ庺ﾇ棂"
    + "ﾑ檚ﾀ誶ﾞ겱ﾮ샀\uffc0샀\uffc0뾿\uffbf뾿\uffbf覉ﾉ\0蔀\0렀ꊢﾢ뾿\uffbf벼ﾼ空ｺ睞ｔ"
    + "䲑ｈ㾴ｓㆋｂ㶮ｖ㦡ｑ㚗ｏ㾰｟捵ｈ뙨ｙꉛｑꉚｓ뙤｟蹍ｋ롢｣鹓ｖ㦚ｪ㶦ｴㆃ～㾩ｼ䮒ｳ"
    + "ꊱﾪ쿏ￏ샀\uffc0뾿\uffbfꊢﾢ\0렀\0찀궭ﾭ뾿\uffbf罿ｿ첅～\ude8a｛岶ｖ只ｰ㲫ｔ僢ｳ"
    + "䧎ｫ䓀ｧ勥ｿ箐｛ｷ클ｩ큲ｬｾ꽞～ﾄ쥩ｯ䧆ﾊ僘ﾛ㲢ｷ叝ﾦ䊰ﾇ䳉ﾞ鳝ￄ\udbdbￛ뾿\uffbf궭ﾭ\0"
    + "찀\0뮻ﾻꦩﾩ祕ｃ\ud987ｘ콿ｖ垫ｓ䷚ｪ㦡ｑ䯓ｭ䓀ｦ䂳｣䳕ｸ璈ｗ\ude7cｰ썫､썪ｦ\ude77"
    + "ｷꕘｚｼ뱡ｪ䒹ﾃ䯉ﾒ㦙ｲ䷍ﾜ㺥ﾀ䚺ﾕ䫃ﾟ떼ﾹ쿏ￏ뮻ﾻ\0\0\uff00뾿\uffbf鲇ﾀ赵ｂ岹ｗ墰ｕ"
    + "㲠ｎ䗄｢㒔ｌ䒾･㺮｟ꑬｔ쉽ｦ鑛ｎ쥯ｧ녠｝녟｟쥫ｭ牵｛徵ﾁ劘ｯ㺧ｺ䒵ﾇ㒌ｫ䖸ﾐ云ｶ덥ｹ뭨ﾀ"
    + "鱍ｨￜ뾿\uffbf\0\uff00\0\uff00뾿\uffbf펐ｯꒊｏ勨ｭ仝ｪ䓀｠嗰ｻ㺮｜叨ｾ䯓ｶ콵ｦ曆ｽ"
    + "둣｜ﾁ홴ｲ홲ｴﾈ羏ｮ嗥ﾢ䚾ﾉ䯊ﾖ叝ﾨ㺥ﾀ嗢ﾳ庭ﾏﾒﾛ뭕ｻￇ뾿\uffbf\0\uff00\0\uff00뾿"
    + "\uffbfꩫｊ籫＾㲩ｑ㦢ｐ㎑ｊ㺮｛ょｉ㲨～㢜ｙ魗ｎ둤｜豌ｉ덡｟ꁕｖꁔｘ덝､扮ｗ㶥"
    + "ｷ㖎ｩ㢕ｱ㲡ｼむ･㺣ﾄ䪁ｮꡎｭ꽐ｳ遁｠잇ﾞ뾿\uffbf\0\uff00\0\uff00뾿\uffbfｨꆇｑ僣ｰ"
    + "䳘ｮ䎼｣哪ｾ㶫｟凣ﾂ䫎ｸ쭱ｨｿ녠～ﾂ퍯ｳ퍮ｶﾉ綌ｰ叟ﾥ䖹ﾋ䫆ﾙ凘ﾪ㶢ﾂ哝ﾶ嶩ﾑﾓﾜ롒ｼﾩ뾿"
    + "\uffbf\0\uff00\0\uff00뾿\uffbf\udd86～酺ｋ䟊ｦ䓀､㲩｛䫐ｳ㞛ｘ䧉ｵ䊸ｮ띤～"
    + "\ud976ｲꅖｖ흲ｶ뵣ｩ뵡ｫ흮ｼ煾ｧ䫆ﾔ㺦ｿ䊰ﾊ䧀ﾚ㞒ｸ䫄ﾤ喗ﾄ졚ﾅ퉝ﾍꝉｲ\udd5fﾙ뾿\uffbf\0"
    + "\uff00\0\uff00뾿\uffbf䧎･㞜ｎ䗃･䊺｣㪤ｚ䣉ｲ榅ｔ칲ｨ뵨｡녠｝퉱ｱ鵓ｖ큭ｴ띟ｨ㺨ｹ"
    + "䞿ﾋ㖏ｫ䢿ﾒ㲡ｾ䂪ﾉ䚹ﾗ虧ｬ퍡ﾊ걎ｲ쉖ﾃ쭙ﾊꉆｰ핛ﾖ뾿\uffbf\0\uff00\0\uff00뾿\uffbf嗰ｹ"
    + "㾲｜凣ｹ䷘ｶ䎼ｪ嗫ﾈ瞘｢ｼ\udc78ｳ쵮ｮﾆ덞｣ﾊ항ｺ䣃ﾎ叟ﾦ㶣ｼ哠ﾮ䚹ﾓ䯆ﾡ勘ﾴ驵ｽﾤ왙ﾅﾚﾤ"
    + "멏ﾂ數ﾱ뾿\uffbf\0\uff00\0\uff00뾿\uffbf㺰ｚㆉｈ㮦ｚ㦟ｙ㎎ｒ㶪･幵ｍ끠｜ꍘｗ"
    + "驒ｔ덞｣譈ｎ뉛ｦ齑｜㚑ｬ㶢ｻ⽾｢㶢ﾁ㖋ｱ㞒ｹ㲝ﾅ癛｢둑ｹ陂ｦꝉｳ깊ｹ輼･띌ﾃ뾿\uffbf\0"
    + "\uff00\0\uff00뾿\uffbf嗯ｾ㾱｠凢ｾ䷗ｻ䎻ｯ嗪ﾎ瞗ｦﾀ\udc75ｷ쵬ｱﾊ덜ｦﾎ핫ｽ䣂ﾓ叞ﾫ"
    + "㶢ﾀ哞ﾴ䚸ﾘ䯅ﾦ勗ﾹ驴ﾀﾨ왗ﾈﾞﾨ멍ﾅ復ﾵ뾿\uffbf\0\uff00\0\uff00뾿\uffbf䧌ｮ㞚ｕ"
    + "䗀ｮ䊸ｫ㪢｢䣆ｺ榃ｚ칮ｯ뵤ｧ녜｣퉬ｷ鵐ｚ큩ｺ띛ｭ㺦ﾀ䞽ﾔ㖍ｲ䢽ﾛ㲟ﾅ䂨ﾐ䚷ﾠ虥ｲ퍜ﾑ걊ｷ쉒ﾉ쭕ﾑ"
    + "ꉃｵ홗ﾝ뾿\uffbf\0\uff00\0\uff00쫊ￊ柍ﾂ㦞ｚ䟆ｴ䒽ｱꥬ｝\udb76ｳꍗｗ핱ｵ썦ｭ띞ｨ"
    + "\ud96fｽ葱､䫅ﾔ䂬ﾄ䂫ﾇ䫂ﾜ㞑ｷ䫃ﾣ㺣ﾌ썘ﾂ핟ﾐꍇｰ\udb5eﾘ녋ｼ쥔ﾏ퉖ﾘ楿ﾋ枼\uffc0뎳ﾳ\0"
    + "\uff00\0\uff00컎ￎ痢ﾓ㾮･僞ﾄ䳓ﾁ뭶ｩﾃ덟｢ﾅ\uda71ｻ쭨ｵﾏ酼ｰ叜ﾨ䢿ﾕ䢾ﾘ叚ﾲ㲟ﾅ叚ﾺ"
    + "䖵ﾝ\uda61ﾒﾣ덎ｼﾬ쑓ﾋﾢﾬ玍ﾛ痐ￖ꾯ﾯ\0\uff00\0퓔ￔ誳ﾕㆈｑ㲤･㦝｣轛ｒ땠｣赊ｎ녜"
    + "､ꑔ～魎｛둚ｫ牡ｚ㶢ｿ㚐ｳ㚐ｵ㶠ﾅぽｪ㶠ﾋ㖊ｺꑈｰ녍ｺ贼｣때ﾁ霿ｭꡄｻ꽆ﾁ屬ｺ誫ﾯ鲜ﾜ\0\0찀쿏"
    + "ￏ꧌ﾲ䂰ｫ勠ﾌ仕ﾈ뽶ｮ廙ﾊ뙟ｦﾋ\udd70ﾀ콧ｺ糧ﾕ鍽ｵ哞ﾱ䧁ﾜ䧀ﾠ哜ﾺ㶠ﾋ嗜ￃ䚷ﾤ\udd60ﾙﾪ뙍ﾁ難ﾴ"
    + "읒ﾑﾩﾳ璎ﾢ꧅\uffc9空ｺ\0찀\0렀컎ￎ쏃ￃ斛ｴ岱ｳ墩ｱ齡｝챪ｲ驏ｗ읥ｳ띛ｫꑠｩ쉰ｽ睮ｦ䒵"
    + "ﾒ㲟ﾃ㲞ﾆ䒳ﾚ䊄ｶ徭ﾟ劑ﾈ띎ｿ읔ﾌ驀ｮ챔ﾔꕅｻ덙ﾏ뭜ﾗ窆ﾔ릹ﾹ䕅ｅ\0렀\0蔀낰ﾰ\udbdbￛ릶ﾶ햀ﾁ"
    + "콭ｱ뙟ｦﾀꝕ｡\udb6eﾁ쥤ｸ䂫ﾄ䳋ﾠ㢕ｸ䳇ﾤ䊮ﾒ䊭ﾕ䳅ﾭ蝮ｸﾚ뱑ﾃ쥕ﾎ\udb5cﾝꝅｹﾦ굗ﾊ䚱ﾺ抹ￄ"
    + "떸ﾸ莃ﾃ〰Ｐ\0蔀\0㐀杧ｧ\uffe7컎ￎ쒼ﾼ튜ﾞ앱ｹﾋ녙ｨﾌ흪ﾂ䒶ﾏ務ﾮ㲝ﾁ凕ﾳ䞹ﾞ䞹ﾢ凓ﾼ轴ﾁ"
    + "ﾧ쥖ﾍ흚ﾙﾪ녈ﾂﾴ빩ﾛ躿ￅ맀ￂ貌ﾌ䱌ｌᤙ９\0㐀\0\0\0툀뮻ﾻ헕ￕ쏃ￃ몺ﾺ붽ﾽ붴ﾴ꾤ﾥ뒞ﾢ낝ﾠ"
    + "蚟ﾕ蒦ﾙ芘ﾐ蒥ﾚ莞ﾖ莝ﾗ蒤ﾝ鎌ﾐ꺇ﾗꚈﾔ난ﾤ뒜ﾦ꾣ﾨ붲ﾷ몺ﾺ꒤ﾤ畵ｵ啕ｕ㸾＾\0팀\0\0\0\0\0㌀"
    + "㴽\ufb3d죈\uffc8쏃ￃ놱ﾱ龟ﾟ鞗ﾗ题ﾘ鶝ﾝ鶝ﾝꢨﾨꦩﾩꦩﾩꦩﾩꦩﾩꦩﾩꦩﾩꦩﾩꦩﾩꢨﾨ鶝ﾝ鶝ﾝ题ﾘ"
    + "袈ﾈ灰ｰ杧ｧ幞～兑ｑတ\ufb10\0㌀\0\0\0\0\0\0\0昀Ⱜ鲜ﾜ놱ﾱ龟ﾟ躎ﾎ膁ﾁ肀ﾀ肀ﾀ肀ﾀ肀ﾀ"
    + "肀ﾀ肀ﾀ肀ﾀ肀ﾀ肀ﾀ肀ﾀ肀ﾀ肀ﾀ肀ﾀ肀ﾀ肀ﾀ祹ｹ灰ｰ杧ｧ䭋ｋ\u0d0d\0昀\0\0\0\0\0\0\0\0"
    + "\0\0\0ⴀ\0숀㌳Ｓ楩ｩ牲ｲ瑴ｴ癶ｶ肀ﾀ肀ﾀ肀ﾀ肀ﾀ肀ﾀ肀ﾀ肀ﾀ肀ﾀ肀ﾀ肀ﾀ肀ﾀ肀ﾀ癶ｶ獳ｳ慡｡䩊"
    + "ｊᰜ＜\0숀\0⸀\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0ἀ\0瀀\0ꈀ\0찀\0픀\0\uff00"
    + "\0\uff00\0\uff00\0\uff00\0\uff00\0\uff00\0\uff00\0\uff00\0\uff00\0"
    + "\uff00\0\uff00\0\uff00\0픀\0찀\0ꈀ\0瀀\0ἀ\0\0\0\0\0\0\0\0\0\0"
    );

file sealed class BinomialMovingAverageFilter<T>
    where T : struct, IFloatingPointIeee754<T>
{
    private readonly T[] mFactors;
    private readonly T[] mValues;

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public BinomialMovingAverageFilter(int size)
    {
        if (size is < 2 or > 256)
        {
            throw new IndexOutOfRangeException(nameof(size));
        }

        mFactors = GC.AllocateUninitializedArray<T>(size);
        mValues = new T[size];

        var factorsSpan = mFactors.AsSpan();
        ref var factorRef = ref MemoryMarshal.GetReference(factorsSpan);
        ref var factorEndRef = ref Unsafe.Add(ref factorRef, factorsSpan.Length);

        factorRef = T.Exp2(T.CreateTruncating(-(--size)));

        var k = 1;
        for (ref var factorNextRef = ref Unsafe.Add(ref factorRef, 1); Unsafe.IsAddressLessThan(ref factorNextRef, ref factorEndRef); factorRef = ref factorNextRef, factorNextRef = ref Unsafe.Add(ref factorNextRef, 1))
        {
            factorNextRef = (T.CreateTruncating(size--) / T.CreateTruncating(k++)) * factorRef;
        }
    }

    public T Value { get; private set; }

    public T Advance(T value)
    {
        var factorsSpan = mFactors.AsSpan();
        var valuesSpan = mValues.AsSpan();

        ref var factorRef = ref MemoryMarshal.GetReference(factorsSpan);
        ref var valueRef = ref MemoryMarshal.GetReference(valuesSpan);
        ref var valueEndRef = ref Unsafe.Add(ref valueRef, valuesSpan.Length);

        var sum = T.Zero;

        for (ref var valueNextRef = ref Unsafe.Add(ref valueRef, 1); Unsafe.IsAddressLessThan(ref valueNextRef, ref valueEndRef); valueRef = ref valueNextRef, valueNextRef = ref Unsafe.Add(ref valueNextRef, 1), factorRef = ref Unsafe.Add(ref factorRef, 1))
        {
            sum += (valueRef = valueNextRef) * factorRef;
        }

        sum += (valueRef = value) * factorRef;

        return Value = sum;
    }
}

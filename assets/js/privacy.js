var en = "<h3>Our Privacy Policy</h3><p>It specifies how to handle the information provided when you use applications <strong>Wallpaper Changer 2</strong>, further - program.</p><p><strong>Image provided by the program.</strong> All images are obtained legally, by using the open capabilities of services or their API, any commercial use of images obtained with the help of the program, without the permission of the service that provided the picture is forbidden!</p><p><strong>Attention!</strong> When installing the program you agree with the text above!</p><p>Yours faithfully,<br /> <strong>Verloka Vadim.</strong></p><p>Translation by Google Translator</p>";
var ru = "<h3>Наша политика конфиденциальности</h3><p>Определяет способ обращения с информацией, предоставляемой при использовании приложения <strong>Wallpaper Changer 2</strong>, далее - программа.</p><p><strong>Изображение которые предоставляет программа.</strong> Все изображения получены законным путем, путем использования открытых возможностей сервисов или их API, любой комерчиское использование изображений, полученых с помощью программы, без разрешения сервиса, предоставившего эту картинку - запрещенно!</p><p><strong>Внимание!</strong> Устанавливая программу Вы соглашаетесь с текстом выше!</p><p>С уважением,<br /> <strong>Верлока Вадим.</strong></p>";
var ua = "<h3>Наша політика конфіденційності</h3><p>Визначає спосіб поводження з інформацією, яка надається при використанні програми <strong>Wallpaper Changer 2</strong>, далі - програма.</p><p><strong>Зображення які надає програма.</strong> Всі зображення отримані законним шляхом, шляхом використання відкритих можливостей сервісів або їх API, будь-яке комерційне використання зображень, отриманих за допомогою програми, без дозволу сервісу, який надав цю картинку - заборонено!</p><p><strong>Увага!</strong> Встановлюючи програму Ви погоджуєтеся з текстом вище!</p><p>З повагою,<br /> <strong>Верлока Вадим.</strong></p>";


function GetPrivacyCode() {
    var $_GET = {};
    var __GET = window.location.search.substring(1).split("&");
    for (var i = 0; i < __GET.length; i++) {
        var getVar = __GET[i].split("=");
        $_GET[getVar[0]] = typeof (getVar[1]) == "undefined" ? "" : getVar[1];
    }
    return $_GET;
}
function GetPrivacyHTML() {
    switch (GetPrivacyCode().p) {
        default:
        case "en":
            return en;
        case "ru":
            return ru;
        case "ua":
            return ua;
    }
}
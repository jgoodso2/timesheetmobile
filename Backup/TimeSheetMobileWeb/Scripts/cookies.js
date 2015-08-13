function setCookie(c_name, value, multivalue, exdays) {
    var exdate = new Date();
    exdate.setDate(exdate.getDate() + exdays);
    if (multivalue) {
        var m_val = '';
        var firstTime = true;
        for (x in value) {
            if (!firstTime) m_val += '&';
            firstTime = false;
            var val = value[x] || '';
            m_val += x + '=' + val;
        }
        value = m_val;
    }
    else value = value;
    var c_value = value + ((exdays == null) ? "" : "; expires=" + exdate.toUTCString());
    document.cookie = c_name + "=" + c_value;
}
function getCookie(c_name, multivalue) {
    var i, x, y, ARRcookies = document.cookie.split(";");
    for (i = 0; i < ARRcookies.length; i++) {
        x = ARRcookies[i].substr(0, ARRcookies[i].indexOf("="));
        y = ARRcookies[i].substr(ARRcookies[i].indexOf("=") + 1);
        x = x.replace(/^\s+|\s+$/g, "");
        if (x == c_name) {
            var c_val = y;
            if (multivalue) {
                var allValues = c_val.split("&");
                var res = {};
                for (var j = 0; j < allValues.length; j++) {
                    var key_val = allValues[j].split("=");
                    res[key_val[0]] = key_val.length>1 ? key_val[1] : '';
                }
                return res;
            }
            else return c_val;
        }
    }
}
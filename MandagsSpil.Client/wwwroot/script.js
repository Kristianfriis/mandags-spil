var mandagsSpil = mandagsSpil || {};
mandagsSpil.setThemeColor = function (isDarkMode) {
    if (isDarkMode) {
        document.querySelector('meta[name="theme-color"]').setAttribute('content', '');
    }
};
// This is a JavaScript module that is loaded on demand. It can export any number of
// functions, and may import other JavaScript modules if required.

export function getBoundingClientRect(elementId) {
    return document.getElementById(elementId).getBoundingClientRect();
};

export function disableMousewheelScroll(elementId) {
    var element = document.getElementById(elementId);
    if (element) {
        element.addEventListener('wheel', function (e) {
            e.preventDefault();
        }, { passive: false });
    }
};

export function disableTouch(elementId) {
    var element = document.getElementById(elementId);
    if (element) {

        element.addEventListener('touchstart', function (e) {
            e.preventDefault();
        }, { passive: false });

        element.addEventListener('touchmove', function (e) {
            e.preventDefault();
        }, { passive: false });

        element.addEventListener('touchend', function (e) {
            e.preventDefault();
        }, { passive: false });
    }
};

export function getPixelDensity() {
    return window.devicePixelRatio || 1;
};

window.sufiAbpCaptcha = {
    loadScript: function (src) {
        return new Promise(function (resolve, reject) {
            if (document.querySelector('script[src="' + src + '"]')) {
                resolve();
                return;
            }

            var script = document.createElement('script');
            script.src = src;
            script.async = true;
            script.defer = true;
            script.onload = function () { resolve(); };
            script.onerror = function () { reject(new Error('Failed to load ' + src)); };
            document.head.appendChild(script);
        });
    },

    renderTurnstile: function (elementId, siteKey, dotnetRef) {
        if (!window.turnstile || !siteKey) {
            return false;
        }

        window.turnstile.render('#' + elementId, {
            sitekey: siteKey,
            callback: function (token) {
                dotnetRef.invokeMethodAsync('OnExternalCaptchaTokenAsync', token);
            },
            'expired-callback': function () {
                dotnetRef.invokeMethodAsync('OnExternalCaptchaTokenAsync', null);
            },
            'error-callback': function () {
                dotnetRef.invokeMethodAsync('OnExternalCaptchaTokenAsync', null);
            }
        });

        return true;
    },

    renderRecaptcha: function (elementId, siteKey, dotnetRef) {
        if (!window.grecaptcha || !siteKey) {
            return false;
        }

        window.grecaptcha.render(elementId, {
            sitekey: siteKey,
            callback: function (token) {
                dotnetRef.invokeMethodAsync('OnExternalCaptchaTokenAsync', token);
            },
            'expired-callback': function () {
                dotnetRef.invokeMethodAsync('OnExternalCaptchaTokenAsync', null);
            }
        });

        return true;
    }
};

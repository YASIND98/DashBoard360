/*!
 * jQuery Loading — sadeleştirilmiş sürüm.
 *
 * Kullanım:
 *   $(el).loading({ message: '...', stoppable: false })   // başlat
 *   $(el).loading('start' | 'stop' | 'toggle' | 'destroy') // metot çağır
 *
 * Tema (`theme`), AMD/CommonJS sarmalı ve özelleştirilebilir onStart/onStop/onClick,
 * hiddenClass/shownClass, custom overlay gibi kullanılmayan özellikler kaldırıldı.
 */
(function ($) {
    'use strict';

    var DATA_KEY = 'jquery-loading';

    var DEFAULTS = {
        message: 'Loading...',
        stoppable: false,
        start: true
    };

    function Loading(element, options) {
        this.element = element;
        this.settings = $.extend({}, DEFAULTS, options);
        this.fullPage = element.is('body');
        this.isActive = false;
        this.overlay = this._createOverlay();
        this.resize();
        this._bindEvents();

        if (this.settings.start) {
            this.start();
        }
    }

    Loading.prototype._createOverlay = function () {
        var overlay = $(
            '<div class="loading-overlay">' +
                '<div class="loading-overlay-content">' + this.settings.message + '</div>' +
            '</div>'
        ).hide().appendTo('body');

        var id = this.element.attr('id');
        if (id) {
            overlay.attr('id', id + '_loading-overlay');
        }
        return overlay;
    };

    Loading.prototype._bindEvents = function () {
        var self = this;

        if (self.settings.stoppable) {
            self.overlay.on('click', function () { self.stop(); });
        }
        $(window).on('resize', function () { self.resize(); });
        $(function () { self.resize(); });
    };

    Loading.prototype.resize = function () {
        var el = this.element;
        var width = this.fullPage ? '100%' : el.outerWidth();
        var height = this.fullPage ? '100%' : el.outerHeight();

        this.overlay.css({
            position: this.fullPage ? 'fixed' : 'absolute',
            zIndex: 150001,
            top: el.offset().top,
            left: el.offset().left,
            width: width,
            height: height
        });
    };

    Loading.prototype.start = function () {
        this.isActive = true;
        this.resize();
        this.overlay.fadeIn(150);
    };

    Loading.prototype.stop = function () {
        this.isActive = false;
        this.overlay.fadeOut(150);
    };

    Loading.prototype.toggle = function () {
        if (this.isActive) {
            this.stop();
        } else {
            this.start();
        }
    };

    Loading.prototype.destroy = function () {
        this.overlay.remove();
    };

    $.fn.loading = function (options) {
        return this.each(function () {
            var instance = $.data(this, DATA_KEY);
            var $el = $(this);

            // İlk çağrı: yeni instance kur
            if (!instance) {
                if (options === undefined || typeof options === 'object' ||
                    options === 'start' || options === 'toggle') {
                    $.data(this, DATA_KEY, new Loading($el, typeof options === 'object' ? options : undefined));
                }
                return;
            }

            // Komut çağrısı: 'start' | 'stop' | 'toggle' | 'destroy'
            if (typeof options === 'string') {
                if (typeof instance[options] === 'function') {
                    instance[options]();
                }
                if (options === 'destroy') {
                    $.removeData(this, DATA_KEY);
                }
                return;
            }

            // Yeni konfig: eski overlay'i yıkıp yeniden kur
            if (typeof options === 'object') {
                instance.destroy();
                $.data(this, DATA_KEY, new Loading($el, options));
                return;
            }

            // Argümansız çağrı: başlat
            instance.start();
        });
    };
})(jQuery);

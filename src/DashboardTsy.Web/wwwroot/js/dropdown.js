function updateCheckboxIcon($item) {
    if ($item.hasClass('checked')) {
        $item.find('.cb-off').hide();
        $item.find('.cb-on').show();
    } else {
        $item.find('.cb-off').show();
        $item.find('.cb-on').hide();
    }
}

function getSelectedCodes(panelId) {
    var codes = [];
    $('.dropdown-panel[data-panel="' + panelId + '"] .dropdown-item:not(.select-all).checked').each(function () {
        codes.push($(this).data('code'));
    });
    return codes;
}

function getSelectedNames(panelId) {
    var names = [];
    $('.dropdown-panel[data-panel="' + panelId + '"] .dropdown-item:not(.select-all).checked').each(function () {
        names.push($(this).find('span').text());
    });
    return names;
}

function clearFilterPanel(panelId) {
    var $panel = $('.dropdown-panel[data-panel="' + panelId + '"]');
    $panel.find('.dropdown-list .dropdown-item:not(.select-all)').remove();
    $panel.find('.select-all').removeClass('checked');
    var $dropdown = $('.filter-dropdown[data-dropdown="' + panelId + '"]');
    $dropdown.find('.filter-badge').text('0').hide();
}

function renderFilterItems($panel, data) {
    var $list = $panel.find('.dropdown-list');
    $list.find('.dropdown-item:not(.select-all)').remove();
    $panel.find('.select-all').removeClass('checked');

    data.forEach(function (item) {
        var $item = $('<label class="dropdown-item" data-code="' + item.code + '">' +
            '<img src="/images/checkbox.svg" class="cb-icon cb-off" alt="" />' +
            '<img src="/images/checkbox-selected.svg" class="cb-icon cb-on" alt="" />' +
            '<span>' + item.name + '</span></label>');
        $list.append($item);
    });

    var panelId = $panel.data('panel');
    $('.filter-dropdown[data-dropdown="' + panelId + '"]').find('.filter-badge').text('0').hide();
}

$(document).ready(function () {

    // Open/close dropdown
    $(document).on('click', '.filter-dropdown', function (e) {
        e.stopPropagation();
        var panelId = $(this).data('dropdown');
        var $panel = $('.dropdown-panel[data-panel="' + panelId + '"]');
        var wasOpen = $panel.hasClass('open');
        $('.dropdown-panel').removeClass('open');
        if (!wasOpen) $panel.addClass('open');
    });

    $(document).on('click', '.dropdown-panel', function (e) {
        if (!$(e.target).closest('.dropdown-item').length) {
            e.stopPropagation();
        }
    });

    $(document).on('click', function (e) {
        if (!$(e.target).closest('.dropdown-panel, .filter-dropdown').length) {
            $('.dropdown-panel').removeClass('open');
        }
    });

    // Checkbox item
    $(document).on('click', '.dropdown-item:not(.select-all)', function (e) {
        e.preventDefault();
        $(this).toggleClass('checked');
        updateCheckboxIcon($(this));
        var $panel = $(this).closest('.dropdown-panel');
        var $items = $panel.find('.dropdown-item:not(.select-all)');
        var $selectAll = $panel.find('.select-all');
        if ($items.length === $items.filter('.checked').length) {
            $selectAll.addClass('checked');
        } else {
            $selectAll.removeClass('checked');
        }
        updateCheckboxIcon($selectAll);
    });

    // Select all
    $(document).on('click', '.dropdown-item.select-all', function (e) {
        e.preventDefault();
        var $this = $(this);
        var $panel = $this.closest('.dropdown-panel');
        var $items = $panel.find('.dropdown-item:not(.select-all)');
        if ($this.hasClass('checked')) {
            $this.removeClass('checked');
            $items.removeClass('checked');
        } else {
            $this.addClass('checked');
            $items.addClass('checked');
        }
        updateCheckboxIcon($this);
        $items.each(function () { updateCheckboxIcon($(this)); });
    });

    // Search
    $(document).on('keyup', '.dropdown-search-input', function () {
        var query = $(this).val().toLowerCase();
        var $items = $(this).closest('.dropdown-panel').find('.dropdown-item:not(.select-all)');
        $items.each(function () {
            $(this).toggle($(this).find('span').text().toLowerCase().indexOf(query) > -1);
        });
    });

    // Kaydet — badge güncelle, kapat, sayfaya event bildir
    $(document).on('click', '.dropdown-save', function () {
        var panelId = $(this).data('panel');
        var $panel = $('.dropdown-panel[data-panel="' + panelId + '"]');
        var $dropdown = $('.filter-dropdown[data-dropdown="' + panelId + '"]');
        var count = $panel.find('.dropdown-item:not(.select-all).checked').length;

        if (count > 0) {
            $dropdown.find('.filter-badge').text(count).show();
        } else {
            $dropdown.find('.filter-badge').hide();
        }

        $panel.removeClass('open');

        $(document).trigger('dropdown:saved', [panelId, getSelectedCodes(panelId)]);
    });

});

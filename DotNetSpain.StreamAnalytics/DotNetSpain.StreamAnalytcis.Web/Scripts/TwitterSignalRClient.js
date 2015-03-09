/// <reference path="jquery.signalR-2.1.2.js" />
/// <reference path="jquery-2.1.3.intellisense.js" />


(function () {

    $(document).ready(function () {

        var twitterHub = $.connection.twitterEventHub;
        twitterHub.client.sendTwitter = function (item) {
            try{
                var ITitter = JSON.parse(item);
                var child = $('#content-twitter').children();
                var element = null;                
                if (ITitter.hasOwnProperty("user") === true) {
                    element = $('<div class="col-md-4"><h2>' + ITitter.user.name + '</h2><p><img src="' + ITitter.user.profile_image_url + '"/></p><p>' + ITitter.text + '<p><p>' + ITitter.created_at + '</p></div>');
                }
                else {
                    element = $('<div class="col-md-4"><h2>' + ITitter.name + '</h2><p><img src="' + ITitter.profile_image_url + '"/></p><p>' + ITitter.text + '<p><p>' + ITitter.created_at + '</p></div>');
                }

                if (child.length === 0) {
                    element.appendTo('#content-twitter');
                }
                else {
                    element.insertBefore(child[0]);
                    setTimeout(clearLastItem, 60000);
                }
            }
            catch (e) {
                console.error(e.toString());
            }
        };

        $.connection.hub.logging = true;
        $.connection.hub.start().done(function (data) {
            var twitterHub = $.connection.twitterEventHub;
            $('#connectionId').html('Connected using ' + data.transport.name);

        });

        $('#select').click(function () {
            var value = $('#source-eventhub').val();
            twitterHub.server.startEventHub(value);
            $("#select").prop("disabled", true);
            $('#source-eventhub').prop("disabled", true);
        });
    });

    var clearLastItem = function () {
        var root = $('#content-twitter');
        var child = root.children();
        child = child.last();
        child.remove();
        child.fadeOut(function () {
            $(this).remove();
        });
    };
})();
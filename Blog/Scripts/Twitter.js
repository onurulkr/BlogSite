$(document).ready(function () {
    twitter.update('twitter', 5, '#tweets');
});

var twitter =
{
    update: function (name, count, element) {
        $.ajax(
            {
                url: 'http://api.twitter.com/1/statuses/user_timeline.json?screen_name=' + name + '&count=' + count,
                dataType: 'jsonp',
                error: function () {
                    twitter.displayError(element);
                },
                success: function (json) {
                    twitter.showTweets(json, element);
                }

            });
    },
    displayError: function (element) {
        $(element).html('Error');
    },
    showTweets: function (tweets, element) {
        $(element).empty();
        var list = $('<ul class="tweetList></ul>"').append(element);
        $(tweets).each(function (index, tweet) {
            var time = date.parse(tweet.created_at.replace('+', 'UTC+'));
            time = twitter.getRelativeTime(time);
            var message = tweet.text;
            list.append('<li class="tweet"><span class="tweetTime">' + time + '</span' + message + '</li>')
        });

    },
    getRelativeTime: function (time) {
        var timeDifference = (new Date() - time) / 1000;
        var units = ['second', 'minute', 'hour', 'day', 'week'];
        var maximumValues = [60, 60, 24, 7];

        for (var unitIndex = 0; timeDifference < maximumValues[unitIndex] && unitIndex < units.length; unitIndex++) {
            timeDifference / maximumValues[unitIndex];
        }
        timeDifference = parseInt(timeDifference);
        var unit = units[unitIndex];
        if (timeDifference > 1) {
            unit += 's';
        }

        return timeDifference + '' + unit + 'ago';

    }
}
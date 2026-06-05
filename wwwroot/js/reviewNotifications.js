(function () {
    "use strict";

    var activeUsersCount = document.getElementById("activeUsersCount");
    var activeUsersIndicator = document.getElementById("activeUsersIndicator");
    var adminActiveUsersCount = document.getElementById("adminActiveUsersCount");
    var toastContainer = document.getElementById("reviewToastContainer");

    function setIndicatorState(state) {
        if (!activeUsersIndicator) {
            return;
        }

        activeUsersIndicator.dataset.state = state;
    }

    function updateActiveUsers(count) {
        if (activeUsersCount) {
            activeUsersCount.textContent = count;
        }

        if (adminActiveUsersCount) {
            adminActiveUsersCount.textContent = count;
        }

        setIndicatorState("connected");
    }

    function updateConnectionIdInputs(connectionId) {
        document.querySelectorAll("form").forEach(function (form) {
            var input = form.querySelector("input[name='SignalRConnectionId']");

            if (!input) {
                input = document.createElement("input");
                input.type = "hidden";
                input.name = "SignalRConnectionId";
                form.appendChild(input);
            }

            input.value = connectionId;
        });
    }

    function getValue(payload, key, fallback) {
        if (!payload || payload[key] === undefined || payload[key] === null || payload[key] === "") {
            return fallback;
        }

        return payload[key];
    }

    function showToast(title, message, movieId) {
        if (!toastContainer || !window.bootstrap) {
            return;
        }

        var toastElement = document.createElement("div");
        toastElement.className = "toast app-toast";
        toastElement.setAttribute("role", "alert");
        toastElement.setAttribute("aria-live", "assertive");
        toastElement.setAttribute("aria-atomic", "true");

        var header = document.createElement("div");
        header.className = "toast-header";

        var titleElement = document.createElement("strong");
        titleElement.className = "me-auto";
        titleElement.textContent = title;

        var timeElement = document.createElement("small");
        timeElement.textContent = "now";

        var closeButton = document.createElement("button");
        closeButton.type = "button";
        closeButton.className = "btn-close";
        closeButton.setAttribute("data-bs-dismiss", "toast");
        closeButton.setAttribute("aria-label", "Close");

        header.appendChild(titleElement);
        header.appendChild(timeElement);
        header.appendChild(closeButton);

        var body = document.createElement("div");
        body.className = "toast-body";

        var messageElement = document.createElement("p");
        messageElement.className = "mb-2";
        messageElement.textContent = message;
        body.appendChild(messageElement);

        if (movieId) {
            var detailsLink = document.createElement("a");
            detailsLink.className = "btn btn-sm btn-outline-light";
            detailsLink.href = "/Movies/Details/" + movieId;
            detailsLink.textContent = "View Details";
            body.appendChild(detailsLink);
        }

        toastElement.appendChild(header);
        toastElement.appendChild(body);
        toastContainer.appendChild(toastElement);

        var toast = new window.bootstrap.Toast(toastElement, {
            autohide: true,
            delay: 6000
        });

        toastElement.addEventListener("hidden.bs.toast", function () {
            toastElement.remove();
        });

        toast.show();
    }

    if (!window.signalR) {
        setIndicatorState("offline");
        return;
    }

    var connection = new window.signalR.HubConnectionBuilder()
        .withUrl("/hubs/reviews")
        .withAutomaticReconnect()
        .build();

    connection.on("ConnectionIdReceived", function (connectionId) {
        updateConnectionIdInputs(connectionId);
    });

    connection.on("ActiveUsersUpdated", function (count) {
        updateActiveUsers(count);
    });

    connection.on("ReviewAdded", function (payload) {
        var movieTitle = getValue(payload, "movieTitle", "a movie");
        var userName = getValue(payload, "userName", "A member");
        var rating = getValue(payload, "rating", "?");
        var movieId = getValue(payload, "movieId", null);

        showToast("New review", userName + " reviewed " + movieTitle + " with " + rating + "/10.", movieId);
    });

    connection.on("MovieRated", function (payload) {
        var movieTitle = getValue(payload, "movieTitle", "a movie");
        var rating = getValue(payload, "rating", "?");
        var movieId = getValue(payload, "movieId", null);

        showToast("Movie rated", movieTitle + " received a new " + rating + "/10 rating.", movieId);
    });

    connection.on("MovieImported", function (payload) {
        var movieTitle = getValue(payload, "movieTitle", "A new movie");
        var releaseYear = getValue(payload, "releaseYear", "");
        var importedBy = getValue(payload, "importedBy", "Admin");
        var movieId = getValue(payload, "movieId", null);
        var yearText = releaseYear ? " (" + releaseYear + ")" : "";

        showToast("Movie imported", importedBy + " imported " + movieTitle + yearText + ".", movieId);
    });

    connection.onreconnecting(function () {
        setIndicatorState("reconnecting");
    });

    connection.onreconnected(function () {
        setIndicatorState("connected");
    });

    connection.onclose(function () {
        setIndicatorState("offline");
    });

    connection.start()
        .then(function () {
            setIndicatorState("connected");
        })
        .catch(function () {
            setIndicatorState("offline");
        });
})();

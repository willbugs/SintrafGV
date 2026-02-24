importScripts('https://www.gstatic.com/firebasejs/5.10.1/firebase-app.js');
importScripts('https://www.gstatic.com/firebasejs/5.10.1/firebase-messaging.js');

var config = {
    apiKey: "AIzaSyAvLiOqGjJMQ2YHGKc1JCO9dGQNjrsjPTc",
    authDomain: "saudde-765e4.firebaseapp.com",
    messagingSenderId: "350306228261",
    projectId: "saudde-765e4"
};

firebase.initializeApp(config);

var messaging = firebase.messaging();

messaging.setBackgroundMessageHandler(function (payload) {
    var dataFromServer = JSON.parse(payload.data.notification);
    var notificationTitle = dataFromServer.title;
    var notificationOptions = {
        body: dataFromServer.body,
        icon: dataFromServer.icon,
        data: {
            url: dataFromServer.url
        }
    };
    return self.registration.showNotification(notificationTitle,
        notificationOptions);
});

self.addEventListener("notificationclick", function (event) {
    var urlToRedirect = event.notification.data.url;
    event.notification.close();
    event.waitUntil(self.clients.openWindow(urlToRedirect));
});
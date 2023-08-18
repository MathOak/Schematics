mergeInto(LibraryManager.library, {
    InternalBase64Generated: function(base64) {
        OnBase64Generated(UTF8ToString(base64));
    },

    InternalGeneratorBootListener: function() {
        OnGeneratorBootListener();
    },

    InternalGeneratorQuitListener: function() {
        OnGeneratorQuitListener();
    },

    InternalUnityErrorLogger: function(errorMessage) {
        console.error("Generator Plugin Process Error:\n" + UTF8ToString(errorMessage));
    },

    InternalUnityLogger: function(message) {
        console.log("Generator Plugin Process Log: " + UTF8ToString(message));
    }
});
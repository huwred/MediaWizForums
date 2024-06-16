(function () {

    'use strict';

    function sendValidationController(
            $scope,
            $http,
            $location,
            editorState,
            navigationService,
            contentResource,
            editorService,
            languageResource
        ) {

        var vm = this;

        vm.member = editorState.current.name;
        vm.node = null;
        vm.loaded = false;

        vm.init = init;
        vm.close = close;
        vm.send = send;

        var currNode = null;



        // ##########################"



        // ## Init dialog
        function init() {
            vm.loaded = true;
        //    // load languages
        //    loadLanguages().then(function (languages) {
        //        vm.languages = languages;

        //        if (vm.languages.length > 0) {
        //            var currCulture = null;
        //            var mainCulture = $location.search().mculture;
        //            if (mainCulture) {
        //                currCulture = _.find(vm.languages, function (l) {
        //                    return l.culture.toLowerCase() === mainCulture.toLowerCase();
        //                });
        //            }
        //            vm.selectedLanguage = currCulture;
        //        }

        //        vm.loaded = true;
        //    });
        }

        // ## Close navigation
        function close() {
            navigationService.hideNavigation();
        }

        // ## Login as the selected member
        function send() {
            // ### Setup cookie
            var url = '/SendValidation';

            // Get the current member id using the editorState
            var _memberId = editorState.current.id;
            console.log_memberId();
            // Do Login
            $http.post(
                url,
                _memberId).then(
                function () {
                    close();
                },
                function (error) { }
            );
        }

    }

    /*
     * @ngdoc Controller
     * @name MediaWizards.SendValidationController
     * 
     * @description
     * Contains the logic of the SendValidation
     * 
     */
    angular.module('umbraco')
        .controller('MediaWizards.SendValidationController', sendValidationController);

})();
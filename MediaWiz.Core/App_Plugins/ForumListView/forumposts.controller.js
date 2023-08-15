(function () {
    "use strict";

    function ForumPostsController($scope, listViewHelper, $location, entityResource) {

        var vm = this;

        vm.selectItem = selectItem;
        vm.clickItem = clickItem;


        // Init the controller
        function activate() {

            // Load parent Topic for each item
            angular.forEach($scope.items, function (item) {
                entityResource.getById(item.parentId,"Document")
                      .then(function(data) {
                          item.parentTopic = data.name;
                      });
            });
            
        }

        // Item select handler
        function selectItem(selectedItem, $event, index) {

            // use the list view helper to select the item
            listViewHelper.selectHandler(selectedItem, index, $scope.items, $scope.selection, $event);
            $event.stopPropagation();

        }

        // Item click handler
        function clickItem(id) {

            // change path to edit item
            $location.path($scope.entityType + '/' + $scope.entityType + '/edit/' + id);

        }

        activate();

    }

    angular.module("umbraco").controller("My.ListView.Layout.ForumPostsController", ForumPostsController);

})();
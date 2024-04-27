design: https://www.figma.com/file/cqubFbZaY5GySbd2kXFzLj/PhotoSorter-v0.2.4?type=design&node-id=9%3A115&mode=design&t=SMXlIHR8iHRglmUu-1

notes:
	- Panel Settings (in UiDocument) has parameter "Scale mode" which should be set to "Constant pixel size"
	(by default is something else) to solve problem with different DPI (or, like in my case, in 125% scale from windows)

bugs:
	- add check if there is folder from config (may be deleted)
	- doesn't support .svg and .bmp
	- have lags when loading big folder (maybe add loading bar)
	- add chech for 'null' in 'DisplayImage'

todo:
	- add GIF animator
	- implement language change
	- add VIDEO support
	- somehow add 'bold' style inside TextElement strings
	- clean and refactor codebase (especially UI related) (+thing about new data structure 'tuple to tuple dictionary')
	- publish final version
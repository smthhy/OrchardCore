/// jQuery helper to append text to a textarea or input.
jQuery.fn.extend({
    insertAtCaret: function (myValue) {
        return this.each(function (i) {
            if (document.selection) {
                //For browsers like Internet Explorer
                this.focus();
                sel = document.selection.createRange();
                sel.text = myValue;
                this.focus();
            } else if (this.selectionStart || this.selectionStart === "0") {
                //For browsers like Firefox and Webkit based
                var startPos = this.selectionStart;
                var endPos = this.selectionEnd;
                var scrollTop = this.scrollTop;
                this.value = this.value.substring(0, startPos) + myValue + this.value.substring(endPos, this.value.length);
                this.focus();
                this.selectionStart = startPos + myValue.length;
                this.selectionEnd = startPos + myValue.length;
                this.scrollTop = scrollTop;
            } else {
                this.value += myValue;
                this.focus();
            }
        });
    }
});

const shortcodeWrapperTemplate = `
<div class="shortcode-popover-wrapper"></div>
`;

const shortcodeHolderTemplate = `
<button type="button" class="shortcode-popover-btn btn btn-sm">
<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 24 24">
    <path d="M16 4.2v1.5h2.5v12.5H16v1.5h4V4.2h-4zM4.2 19.8h4v-1.5H5.8V5.8h2.5V4.2h-4l-.1 15.6zm5.1-3.1l1.4.6 4-10-1.4-.6-4 10z"></path>
</svg>
</button>
<div class="shortcode-popover-holder mt-n3 mr-n3 py-3 px-2 w-50 bg-white border shadow rounded" style="display:none"></div>  
`;

// Wraps each .shortcode-popover class with a wrapper, and attaches detaches the shortcode app as required.
$(function () {
    $('.shortcode-popover').each(function () {
        $(this).wrap(shortcodeWrapperTemplate);
        $(this).parent().append(shortcodeHolderTemplate);
    });

    $('.shortcode-popover-btn').on('click', function() {
        const holder = $(this).siblings('.shortcode-popover-holder');

        shortcodeApp.init(holder, $(this).siblings('.shortcode-popover'), true); 
        
        holder.fadeToggle();
    
        const modalCloser = function (e) {
            if (!holder.is(e.target) && holder.has(e.target).length === 0) 
            {
                cancel();
            }
        }
    
        const cancel = function() {
            holder.fadeToggle();
            
            $(document).off('mouseup', modalCloser);            
        }
    
        $(document).on('mouseup', modalCloser);
    
        $('#shortcode-popover-app-content').on('success', function () {
            if (shortcodeApp.value && shortcodeApp.value.defaultShortcode) {
                var input = $(holder).siblings('.shortcode-popover');
                input.insertAtCaret(shortcodeApp.value.defaultShortcode);
            }
            holder.fadeToggle();
          
            $(document).off('mouseup', modalCloser);
        })
        $('#shortcode-popover-app-content').on('cancel', cancel);        
    });
})

var shortcodesApp;

function initializeShortcodesApp(element) {
    if (element) {
        var elementId = element.id;

        shortcodesApp = new Vue({
            el: '#' + elementId,
            data : {
                returnShortcode: ''
            },
            methods: {
                init()
                {
                    this.selectedValue = '';
                },
                insertShortcode(returnShortcode) {
                    this.returnShortcode = returnShortcode;
                    $(this.$el).modal('hide')

                }
            }
        });

        return shortcodeApp;
    }
}


// initializes a code mirror editor with a shortcode popover.
// categories should be placed on the parent with class .shortcode-popover-categories.
function initializeCodeMirrorShortcodeWrapper(editor) {
    const codemirrorWrapper = editor.display.wrapper;

    $(codemirrorWrapper).wrap(shortcodeWrapperTemplate);
    $(codemirrorWrapper).parent().append(shortcodeHolderTemplate);
    $(codemirrorWrapper).siblings('.shortcode-popover-btn').on('click', function () {
        const holder = $(this).siblings('.shortcode-popover-holder');

        shortcodeApp.init(holder, $(this).closest('.shortcode-popover-categories'), true); 
        
        holder.fadeToggle();
    
        const modalCloser = function (e) {
            if (!holder.is(e.target) && holder.has(e.target).length === 0) 
            {
                cancel();
            }
        }
    
        const cancel = function() {
            holder.fadeToggle();
            
            $(document).off('mouseup', modalCloser);            
        }
    
        $(document).on('mouseup', modalCloser);

        // By design these leave the popover in place where another editor can move it.
        $('#shortcode-popover-app-content').on('success', function () {
            if (shortcodeApp.value && shortcodeApp.value.defaultShortcode) {
                editor.replaceSelection(shortcodeApp.value.defaultShortcode);    
            }
            holder.fadeToggle();
          
            $(document).off('mouseup', modalCloser);
        })
        $('#shortcode-popover-app-content').on('cancel', cancel);        
    });  
}

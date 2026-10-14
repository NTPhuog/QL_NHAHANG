(() => {
 const input=document.querySelector('[data-image-input]'), preview=document.querySelector('[data-image-preview]'), error=document.querySelector('[data-image-error]');
 if(!input)return;const previous=preview.getAttribute('src');let url;
 input.addEventListener('change',()=>{if(url)URL.revokeObjectURL(url);const file=input.files[0];error.textContent='';preview.src=previous||'';preview.hidden=!previous;if(!file)return;if(file.size>5*1024*1024||!['image/jpeg','image/png','image/webp'].includes(file.type)){error.textContent='Chọn ảnh JPG, PNG hoặc WebP không quá 5 MB.';input.value='';return;}url=URL.createObjectURL(file);preview.src=url;preview.hidden=false;});
})();

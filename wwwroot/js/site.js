
GetSongs();
GetGenres();

function GetSongs() {
    $.ajax({
            url: 'https://localhost:7186/api/songs',
            method: "GET",
            contentType: "application/json",
            success: function (songs) {
                let rows = "";
                $.each(songs, function (index, song) {
                    // добавляем полученные элементы в таблицу
                    rows += row(song);
                })
                $("table tbody").append(rows);
            },
            error: function (x) {
                alert(x.status);
            }
        });
}

//получение жанров
function GetGenres(selectedGenre) {
    $.ajax({
            url: 'https://localhost:7186/api/genres',
            method: "GET",
            contentType: "application/json",
            success: function (genres) {
                //создаем select
                let select = document.getElementById("selGenres");
                select.innerHTML = "";             //очищаем селект                  
                $.each(genres, function (index, genre) {
                    //создаем option
                    let option = document.createElement("option");
                    option.value = genre.id;
                    option.text = genre.name;
                    if (genre.id === selectedGenre) {
                        option.selected = true;
                    }
                    select.appendChild(option);
                }); 
            },
            error: function (x) {
                alert(x.status);
            }
        });
}

let row = function (song) {
    return "<tr data-rowid='" + song.id + "'><td>" + song.id +
        "</td>" + "<td>" + song.name + "</td> <td>" + song.author +
        "</td>" + "<td>" + song.genre + "</td><td>" + song.path + "</td>"
        + "<td><a class='editLink' data-id='" + song.id + "'>Изменить</a> | " +
        "<a class='removeLink' data-id='" + song.id + "'>Удалить</a></td></tr>";
}

function GetSong(id) {
    $.ajax({
            url: 'https://localhost:7186/api/songs/' + id,
            method: "GET",
            contentType: "application/json",
            success: function (song) {
                GetGenres(song.genreId);
                let form = document.forms["songForm"];
                form.elements["id"].value = song.id;
                form.elements["name"].value = song.name;
                form.elements["author"].value = song.author;
                form.elements["path"].value = song.path;
            },
            error: function (x) {
                alert(x.status);
            }
        });
}

function EditSong(songId, songName, songAuthor, songGenreId, songGenre, songPath) {
    let request = JSON.stringify({
            id: songId,
            name: songName,
            author: songAuthor,
            genreId: songGenreId,
            genre: songGenre,
            path: songPath
        });
    $.ajax({
            url: "https://localhost:7186/api/songs",
            contentType: "application/json",
            method: "PUT",
            data: request,
            success: function (song) {
                $("tr[data-rowid='" + song.id + "']").replaceWith(row(song));
                //let select = document.getElementById("selGenres");
                //select.innerHTML = "";
                let inputFile = document.getElementById("inputFile");
                inputFile.setAttribute("style", "visibility: visible;")                
                let form = document.forms["songForm"];
                form.reset();
                form.elements["id"].value = 0;
            },
            error: function (x) {
                alert(x.status);
            }
        })
}

function CreateSong(songName, songAuthor, songGenreId, songGenre, file) {
    const fileInput = document.getElementById("inputFile");
    if (fileInput.files.length === 0)

{
    alert("Выберите файл!");
    return;
}

const formData = new FormData();
formData.append("name", songName);
formData.append("author", songAuthor);
formData.append("genreId", songGenreId);
formData.append("genre", songGenre);
formData.append("file", file);

$.ajax({
            url: "https://localhost:7186/api/songs",
            method: "POST",
            processData: false, // Важно для FormData
            contentType: false, // Важно для FormData
            data: formData,
            success: function (song) {
                $("table tbody").append(row(song));
                let select = document.getElementById("selGenres");
                select.innerHTML = "";
                let inputFile = document.getElementById("inputFile");
                inputFile.setAttribute("style", "visibility: visible;")
                let form = document.forms["songForm"];
                form.reset();
                form.elements["id"].value = 0;
            },
            error: function (x) {
                alert(x.status);
            }
        });
}

function DeleteSong(id) {
    if (!confirm("Удалить песню?")) return;
    $.ajax({
            url: "https://localhost:7186/api/songs/" + id,
            contentType: "application/json",
            method: "DELETE",
            success: function (id) {
                $("tr[data-rowid='" + id + "']").remove();
            },
            error: function (x, y, z) {
                alert(x.status + '\n' + y + '\n' + z);
            }
        })
}

//очищаем форму
$("#reset").click(function (e) {
        e.preventDefault();
        //let select = document.getElementById("selGenres");
        //select.innerHTML = "";
        let inputFile = document.getElementById("inputFile");
        inputFile.setAttribute("style", "visibility: visible;")
        let form = document.forms["songForm"];
        form.reset();
        form.elements["id"].value = 0;
    });
//сохраняем изменения
$("#submit").click(function (e) {
        e.preventDefault();
        let select = document.getElementById("selGenres");
        let form = document.forms["songForm"];
        let songId = form.elements["id"].value;
        let songName = form.elements["name"].value;
        let songAuthor = form.elements["author"].value;
        let songPath = form.elements["path"].value;
        let songGenreId = select.value;
        let songGenre = select.options[select.selectedIndex].text;
        const fileInput = document.getElementById("inputFile");
        let file = fileInput.files[0];
        if (songId == 0) 
            CreateSong(songName, songAuthor, songGenreId, songGenre, file);        
        else
            EditSong(songId, songName, songAuthor, songGenreId, songGenre, songPath);               
    });

// нажимаем на ссылку Изменить
$("body").on("click", ".editLink", function () {       
        let id = $(this).data("id");
        let inputFile = document.getElementById("inputFile");
        inputFile.setAttribute("style", "visibility: hidden;")
        GetSong(id);
    });
// нажимаем на ссылку Удалить
$("body").on("click", ".removeLink", function () {
        let id = $(this).data("id");
        DeleteSong(id);
    });
